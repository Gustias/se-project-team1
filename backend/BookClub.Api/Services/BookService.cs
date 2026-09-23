using System.Text.Json;
using System.Text.Json.Serialization;

public class BookService
{
    private readonly HttpClient _httpClient;

    public BookService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<BookSearchResultDto>> SearchBooksAsync(
        string query, CancellationToken cancellationToken = default
    )
    {
        var encodedQuery = Uri.EscapeDataString(query);

        var url = $"search.json?q={encodedQuery}&limit=20";

        using var response = await _httpClient.GetAsync(url, cancellationToken);

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        var openLibraryResponse = await JsonSerializer.DeserializeAsync<OpenLibrarySearchResponse>
        (stream, cancellationToken: cancellationToken);

        if(openLibraryResponse?.Docs == null)
        {
            return new List<BookSearchResultDto>();
        }

        return openLibraryResponse.Docs
        .Where(book => !string.IsNullOrWhiteSpace(book.Title))
        .Select(book => new BookSearchResultDto
        {
            ExternalId = book.Key ?? "",
            Title = book.Title ?? "",
            Author = book.AuthorName != null
                ? string.Join(", ", book.AuthorName)
                : "",
            CoverUrl = book.CoverI.HasValue
                ? $"https://covers.openlibrary.org/b/id/{book.CoverI}-S.jpg"
                : null
        })

        .ToList();
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
    }
}