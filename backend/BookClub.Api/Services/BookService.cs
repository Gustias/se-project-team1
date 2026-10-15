using System.Text.Json;
using System.Text.Json.Serialization;
using BookClub.Api.Data;
using BookClub.Api.DTOs;
using BookClub.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using System.Text;
using System.Globalization;

namespace BookClub.Api.Services;

public class BookService
{
    private readonly AppDbContext _dbContext;
    private readonly HttpClient _httpClient;
    public BookService(AppDbContext dbContext, HttpClient httpClient)
    {
        _dbContext = dbContext;
        _httpClient = httpClient;
    }

    public async Task<List<BookSearchResultDto>> SearchBooksAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        var normalizedQuery = query.Trim();

        var searchTerms = normalizedQuery
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var escapedQuery = string.Join(
            " AND ",
            searchTerms
                .SelectMany(term => new[]
                {
                    term,
                    RemoveDiacritics(term)
                })
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(EscapeSearchQuery));

        var fields =
            "key,title,author_name,cover_i,editions,editions.key," +
            "editions.title,editions.cover_i,editions.isbn_10,editions.isbn_13";

        var encodedQuery = Uri.EscapeDataString(escapedQuery);

        var url = $"search.json?q={encodedQuery}&limit=100&fields={fields}";

        using var response =
            await _httpClient.GetAsync(url, cancellationToken);

        response.EnsureSuccessStatusCode();

        await using var stream =
            await response.Content.ReadAsStreamAsync(cancellationToken);

        var openLibraryResponse =
            await JsonSerializer.DeserializeAsync<OpenLibrarySearchResponse>(
                stream,
                cancellationToken: cancellationToken);

        if (openLibraryResponse?.Docs == null)
        {
            return new List<BookSearchResultDto>();
        }

        var normalizedTerms = searchTerms
            .Select(RemoveDiacritics)
            .ToList();

        var results = openLibraryResponse.Docs
            .Where(book => !string.IsNullOrWhiteSpace(book.Title))
            .Where(book => !IsCollectionTitle(book.Title!))
            .Where(book =>
            {
                var title = RemoveDiacritics(book.Title!);

                var authors = book.AuthorName?
                    .Select(RemoveDiacritics)
                    .ToList() ?? new List<string>();

                var titleMatch = normalizedTerms.All(term =>
                    title.Contains(term, StringComparison.OrdinalIgnoreCase));

                var authorMatch = authors.Any(author =>
                    normalizedTerms.All(term =>
                        author.Contains(term, StringComparison.OrdinalIgnoreCase)));

                var combinedMatch = normalizedTerms.Count > 1 &&
                    normalizedTerms.All(term =>
                        title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                        authors.Any(author =>
                            author.Contains(term, StringComparison.OrdinalIgnoreCase)));

                return titleMatch || authorMatch || combinedMatch;
            })

            .OrderByDescending(book =>
                string.Equals(
                    book.Title?.Trim(),
                    normalizedQuery,
                    StringComparison.Ordinal))
            .ThenByDescending(book =>
                string.Equals(
                    book.Title?.Trim(),
                    normalizedQuery,
                    StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(book =>
                string.Equals(
                   RemoveDiacritics(book.Title ?? ""),
                   RemoveDiacritics(normalizedQuery),
                   StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(book =>
                book.CoverI.HasValue || book.Editions?.Docs?.Any(edition =>
                    edition.CoverI.HasValue) == true)

            .Take(30)
            .Select(book =>
            {
                var editions = book.Editions?.Docs ?? new List<OpenLibraryEdition>();

                var edition = editions.FirstOrDefault(edition =>
                    edition.CoverI.HasValue);

                edition ??= editions.FirstOrDefault(edition =>
                    !string.IsNullOrEmpty(edition.Key));

                var externalId = edition?.Key ?? book.Key;

                if (string.IsNullOrWhiteSpace(externalId))
                {
                    return null;
                }

                return new BookSearchResultDto
                {
                    ExternalId = externalId,
                    Title = book.Title ?? edition?.Title ?? "",
                    Author = book.AuthorName != null
                        ? string.Join(", ", book.AuthorName)
                        : "",
                    CoverUrl = edition?.CoverI.HasValue == true || book.CoverI.HasValue
                        ? $"https://covers.openlibrary.org/b/id/{edition?.CoverI ?? book.CoverI}-M.jpg"
                        : null
                };
            })
            .Where(result => result != null)
            .Select(result => result!)
            .GroupBy(result => new
            {
                Title = RemoveDiacritics(result.Title).Trim().ToLowerInvariant(),
                Author = RemoveDiacritics(result.Author).Trim().ToLowerInvariant()
            })
            .Select(group => group.First())
            .ToList();

        return results;
    }

    private class OpenLibrarySearchResponse
    {
        [JsonPropertyName("docs")]
        public List<OpenLibraryBook> Docs { get; set; } = new();
    }

    private class OpenLibraryBook
    {
        [JsonPropertyName("key")]
        public string? Key { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("author_name")]
        public List<string>? AuthorName { get; set; }

        [JsonPropertyName("cover_i")]
        public int? CoverI { get; set; }

        [JsonPropertyName("editions")]
        public OpenLibraryEditions? Editions { get; set; }
    }

    private class OpenLibraryEditions
    {
        [JsonPropertyName("docs")]
        public List<OpenLibraryEdition>? Docs { get; set; }
    }

    private class OpenLibraryEdition
    {
        [JsonPropertyName("key")]
        public string? Key { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("cover_i")]
        public int? CoverI { get; set; }

        [JsonPropertyName("isbn_10")]
        public List<string>? Isbn10 { get; set; }

        [JsonPropertyName("isbn_13")]
        public List<string>? Isbn13 { get; set; }
    }

    public async Task<GetBookDto?> CreateAsync(
        string externalId,
        string? isbn10,
        string? isbn13,
        string title,
        string? author)
    {
        var existing = await _dbContext.Books
            .FirstOrDefaultAsync(b =>
                b.ExternalId == externalId);

        if (existing is not null)
        {
            return null;
        }

        var entry = new Book
        {
            ExternalId = externalId,
            Isbn10 = isbn10,
            Isbn13 = isbn13,
            Title = title,
            Author = author
        };

        _dbContext.Books.Add(entry);
        await _dbContext.SaveChangesAsync();

        return ToDto(entry);

    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _dbContext.Books.FindAsync(id);

        if (existing is null)
        {
            return false;
        }

        _dbContext.Books.Remove(existing);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<GetBookDto?> GetAsync(int id)
    {
        var existing = await _dbContext.Books.FindAsync(id);

        if (existing is null)
        {
            return null;
        }

        return ToDto(existing);
    }

    private static string RemoveDiacritics(string text)
    {
        var normalized = text.Normalize(NormalizationForm.FormD);

        var characters = normalized
            .Where(character =>
                CharUnicodeInfo.GetUnicodeCategory(character)
                != UnicodeCategory.NonSpacingMark);
        return new string(characters.ToArray())
            .Normalize(NormalizationForm.FormC);
    }

    private static bool IsCollectionTitle(string title)
    {

        var normalizedTitle = title.ToLowerInvariant();

        var collectionIndicators = new[]
        {
            "complete series",
            "complete collection",
            "complete works",
            "short stories",
            "selected stories",
            "summary of",
            "study guide",
            "collected works",
            "collected edition",
            "collected stories",
            "omnibus",
            "box set",
            "boxed set",
            "anthology",
            "anthologies",
            "novels by"
        };

        if (collectionIndicators.Any(indicator =>
            normalizedTitle.Contains(indicator)))
        {
            return true;
        }

        if (title.Count(character => character == ',') >= 2)
        {
            return true;
        }

        return false;

    }

    private static string EscapeSearchQuery(string query)
    {
        return Regex.Replace(
            query,
            @"(\+|-|&&|\|\||!|\(|\)|\{|\}|\[|\]|\^|""|~|\*|\?|:|\\|/)",
            @"\$1");
    }

    private static GetBookDto ToDto(Book entry)
    {
        return new GetBookDto
        {
            Id = entry.Id,
            ExternalId = entry.ExternalId,
            Isbn10 = entry.Isbn10,
            Isbn13 = entry.Isbn13,
            Title = entry.Title,
            Author = entry.Author
        };
    }

}
