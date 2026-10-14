using System.Text.Json;
using System.Text.Json.Serialization;
using BookClub.Api.Data;
using BookClub.Api.DTOs;
using BookClub.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

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
        var escapedQuery = EscapeSearchQuery(normalizedQuery);

        var searchQuery = $"title:{escapedQuery} OR author:{escapedQuery}";
        var encodedQuery = Uri.EscapeDataString(searchQuery);

        var fields =
            "key,title,author_name,cover_i,editions,editions.key," +
            "editions.title,editions.cover_i,editions.isbn_10,editions.isbn_13";

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

        var results = openLibraryResponse.Docs
            .Where(book => !string.IsNullOrWhiteSpace(book.Title))
            .Where(book => !IsCollectionTitle(book.Title!))
            .Where(book =>
                book.Title!.Contains(
                    normalizedQuery,
                    StringComparison.OrdinalIgnoreCase)
                ||
                (book.AuthorName?.Any(author =>
                    author.Contains(
                        normalizedQuery,
                        StringComparison.OrdinalIgnoreCase)) ?? false))
            .Where(book => book.Editions?.Docs?.Any(edition =>
                !string.IsNullOrWhiteSpace(edition.Key)) == true)
            .OrderByDescending(book =>
                book.AuthorName?.Any(author =>
                    author.Equals(
                        normalizedQuery,
                        StringComparison.OrdinalIgnoreCase)) == true)
            .Take(30)
            .Select(book =>
            {
                var edition = book.Editions!.Docs!
                .First(edition => !string.IsNullOrWhiteSpace(edition.Key));

                return new BookSearchResultDto
                {
                    ExternalId = edition.Key!,
                    Title = book.Title ?? edition.Title ?? "",
                    Author = book.AuthorName != null
                        ? string.Join(", ", book.AuthorName)
                        : "",
                    CoverUrl = edition.CoverI.HasValue || book.CoverI.HasValue
                        ? $"https://covers.openlibrary.org/b/id/{edition.CoverI ?? book.CoverI}-M.jpg"
                        : null
                };
            })
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
