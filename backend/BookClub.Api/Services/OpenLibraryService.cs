using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using BookClub.Api.DTOs;

namespace BookClub.Api.Services;

public class OpenLibraryService
{
    private readonly HttpClient _httpClient;

    public OpenLibraryService(HttpClient httpClient)
    {
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

        var url = $"search.json?q={encodedQuery}&limit=100" +
            "&fields=key,title,author_name,cover_i,first_publish_year,isbn,publisher";

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

        return openLibraryResponse.Docs
            .Where(book => !string.IsNullOrWhiteSpace(book.Title))
            .Where(book =>
                book.Title!.Contains(
                    normalizedQuery,
                    StringComparison.OrdinalIgnoreCase)
                ||
                (book.AuthorName?.Any(author =>
                    author.Contains(
                        normalizedQuery,
                        StringComparison.OrdinalIgnoreCase)) ?? false))
            .Take(30)
            .Select(book =>
            {
                var (isbn10, isbn13) = PickIsbns(book.Isbn);

                return new BookSearchResultDto
                {
                    ExternalId = book.Key ?? "",
                    Title = book.Title ?? "",
                    Author = book.AuthorName != null
                        ? string.Join(", ", book.AuthorName)
                        : "",
                    CoverUrl = book.CoverI.HasValue
                        ? $"https://covers.openlibrary.org/b/id/{book.CoverI}-M.jpg"
                        : null,
                    Year = book.FirstPublishYear,
                    Isbn10 = isbn10,
                    Isbn13 = isbn13,
                    Publishers = (book.Publishers ?? new List<string>())
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .Take(10)
                        .ToList()
                };
            })
            .ToList();
    }

    private static string EscapeSearchQuery(string query)
    {
        return Regex.Replace(
            query,
            @"(\+|-|&&|\|\||!|\(|\)|\{|\}|\[|\]|\^|""|~|\*|\?|:|\\|/)",
            @"\$1");
    }

    private static (string? Isbn10, string? Isbn13) PickIsbns(
        List<string>? isbns)
    {
        if (isbns is null)
        {
            return (null, null);
        }

        var cleaned = isbns
            .Select(i => i.Replace("-", "").Trim())
            .ToList();

        return (
            cleaned.FirstOrDefault(i => i.Length == 10),
            cleaned.FirstOrDefault(i => i.Length == 13)
        );
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

        [JsonPropertyName("first_publish_year")]
        public int? FirstPublishYear { get; set; }

        [JsonPropertyName("isbn")]
        public List<string>? Isbn { get; set; }

        [JsonPropertyName("publisher")]
        public List<string>? Publishers { get; set; }
    }
}