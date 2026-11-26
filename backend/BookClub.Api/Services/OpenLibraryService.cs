using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using BookClub.Api.DTOs;

namespace BookClub.Api.Services;

public class OpenLibraryService
{
    private readonly HttpClient _httpClient;
    private readonly OpenLibrarySearchCache _cache;

    public OpenLibraryService(
        HttpClient httpClient,
        OpenLibrarySearchCache cache)
    {
        _httpClient = httpClient;
        _cache = cache;
    }

    public async Task<List<BookSearchResultDto>> SearchBooksAsync(
        string query,
        CancellationToken cancellationToken = default,
        int limit = 100)
    {
        var normalizedQuery = query.Trim();

        return await _cache.GetOrCreateAsync(
            normalizedQuery,
            () => FetchBooksFromOpenLibraryAsync(
                normalizedQuery,
                limit,
                cancellationToken),
            cancellationToken);
    }

    private async Task<List<BookSearchResultDto>> FetchBooksFromOpenLibraryAsync(
        string normalizedQuery,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var isIsbn = TryNormalizeIsbn(
            normalizedQuery,
            out var isbnQuery);

        string searchQuery;

        if (isIsbn)
        {
            searchQuery = $"isbn:{isbnQuery}";
        }
        else
        {
            var escapedQuery =
                EscapeSearchQuery(normalizedQuery);

            searchQuery =
                $"title:{escapedQuery} OR author:{escapedQuery}";
        }

        var encodedQuery =
            Uri.EscapeDataString(searchQuery);

        var url =
            $"search.json?q={encodedQuery}&limit={limit}" +
            "&fields=key,title,author_name,cover_i,first_publish_year,isbn,publisher";

        using var response =
            await _httpClient.GetAsync(
                url,
                cancellationToken);

        response.EnsureSuccessStatusCode();

        await using var stream =
            await response.Content.ReadAsStreamAsync(
                cancellationToken);

        var openLibraryResponse =
            await JsonSerializer.DeserializeAsync<OpenLibrarySearchResponse>(
                stream,
                cancellationToken: cancellationToken);

        if (openLibraryResponse?.Docs == null)
        {
            return new List<BookSearchResultDto>();
        }

        var docs = openLibraryResponse.Docs
            .Where(book =>
                !string.IsNullOrWhiteSpace(book.Title));

        if (!isIsbn)
        {
            docs = docs.Where(book =>
                book.Title!.Contains(
                    normalizedQuery,
                    StringComparison.OrdinalIgnoreCase)
                ||
                (book.AuthorName?.Any(author =>
                    author.Contains(
                        normalizedQuery,
                        StringComparison.OrdinalIgnoreCase))
                ?? false));
        }

        return docs
            .Take(30)
            .Select(book =>
            {
                var (isbn10, isbn13) =
                    PickIsbns(book.Isbn);

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
                    Publishers =
                        (book.Publishers ?? new List<string>())
                        .Distinct(
                            StringComparer.OrdinalIgnoreCase)
                        .Take(10)
                        .ToList()
                };
            })
            .ToList();
    }

    public async Task<WorkDetailsDto?> GetWorkAsync(
        string workId,
        CancellationToken cancellationToken = default)
    {
        var url = $"works/{Uri.EscapeDataString(workId)}.json";

        using var response =
            await _httpClient.GetAsync(url, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        await using var stream =
            await response.Content.ReadAsStreamAsync(cancellationToken);

        var work =
            await JsonSerializer.DeserializeAsync<OpenLibraryWork>(
                stream,
                cancellationToken: cancellationToken);

        if (work is null || string.IsNullOrWhiteSpace(work.Title))
        {
            return null;
        }

        return new WorkDetailsDto
        {
            ExternalId = workId,
            Title = work.Title,
            Description = ReadDescription(work.Description),
            CoverUrl = work.Covers?.FirstOrDefault(c => c > 0) is int coverId
                ? $"https://covers.openlibrary.org/b/id/{coverId}-L.jpg"
                : null,
            FirstPublishDate = work.FirstPublishDate
        };
    }

    public async Task<PagedResult<EditionSearchResultDto>?> SearchEditionsAsync(
        string workId,
        CancellationToken cancellationToken = default,
        int limit = 20,
        int offset = 0)
    {
        var url = $"works/{Uri.EscapeDataString(workId)}/editions.json" +
            $"?limit={limit}&offset={offset}";

        using var response =
            await _httpClient.GetAsync(url, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        await using var stream =
            await response.Content.ReadAsStreamAsync(cancellationToken);

        var openLibraryResponse =
            await JsonSerializer.DeserializeAsync<OpenLibraryEditionsResponse>(
                stream,
                cancellationToken: cancellationToken);

        var entries = openLibraryResponse?.Entries ?? new List<OpenLibraryEdition>();

        var items = entries
            .Where(e => !string.IsNullOrWhiteSpace(e.Title))
            .Select(e => new EditionSearchResultDto
            {
                ExternalId = e.Key?.Split('/').Last() ?? "",
                WorkExternalId = workId,
                Title = e.Title ?? "",
                Subtitle = e.Subtitle,
                CoverUrl = e.Covers?.FirstOrDefault(c => c > 0) is int coverId
                    ? $"https://covers.openlibrary.org/b/id/{coverId}-M.jpg"
                    : null,
                PublishDate = e.PublishDate,
                Isbn10 = e.Isbn10?.FirstOrDefault(),
                Isbn13 = e.Isbn13?.FirstOrDefault(),
                Publishers = (e.Publishers ?? new List<string>())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Take(10)
                    .ToList(),
                PageCount = e.NumberOfPages,
                Format = e.PhysicalFormat,
                Language = e.Languages?.FirstOrDefault()?.Key?.Split('/').Last()
            })
            .ToList();

        return new PagedResult<EditionSearchResultDto>
        {
            Total = openLibraryResponse?.Size ?? offset + entries.Count,
            Items = items
        };
    }

    // Open Library returns the description either as a plain string
    // or as an object like { "type": "/type/text", "value": "..." }.
    private static string? ReadDescription(JsonElement? description)
    {
        if (description is not { } value)
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Object
                when value.TryGetProperty("value", out var text)
                    && text.ValueKind == JsonValueKind.String => text.GetString(),
            _ => null
        };
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

    private static bool TryNormalizeIsbn(string input, out string isbn)
    {
        // Remove hyphens and spaces: "978-0-14-032872-1" -> "9780140328721"
        isbn = input.Replace("-", "").Replace(" ", "").ToUpperInvariant();
        return Regex.IsMatch(isbn, @"^(\d{13}|\d{9}[\dX])$");
    }

    private class OpenLibrarySearchResponse
    {
        [JsonPropertyName("docs")]
        public List<OpenLibraryBook> Docs { get; set; } = new();
    }

    private class OpenLibraryWork
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("description")]
        public JsonElement? Description { get; set; }

        [JsonPropertyName("covers")]
        public List<int>? Covers { get; set; }

        [JsonPropertyName("first_publish_date")]
        public string? FirstPublishDate { get; set; }
    }

    private class OpenLibraryEditionsResponse
    {
        [JsonPropertyName("size")]
        public int? Size { get; set; }

        [JsonPropertyName("entries")]
        public List<OpenLibraryEdition>? Entries { get; set; }
    }

    private class OpenLibraryEdition
    {
        [JsonPropertyName("key")]
        public string? Key { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("covers")]
        public List<int>? Covers { get; set; }

        [JsonPropertyName("publish_date")]
        public string? PublishDate { get; set; }

        [JsonPropertyName("isbn_10")]
        public List<string>? Isbn10 { get; set; }

        [JsonPropertyName("isbn_13")]
        public List<string>? Isbn13 { get; set; }

        [JsonPropertyName("publishers")]
        public List<string>? Publishers { get; set; }

        [JsonPropertyName("number_of_pages")]
        public int? NumberOfPages { get; set; }

        [JsonPropertyName("subtitle")]
        public string? Subtitle { get; set; }

        [JsonPropertyName("physical_format")]
        public string? PhysicalFormat { get; set; }

        [JsonPropertyName("languages")]
        public List<OpenLibraryKeyRef>? Languages { get; set; }
    }

    private class OpenLibraryKeyRef
    {
        [JsonPropertyName("key")]
        public string? Key { get; set; }
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
