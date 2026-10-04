using System.Text.Json;
using System.Text.Json.Serialization;
using BookClub.Api.Data;
using BookClub.Api.DTOs;
using BookClub.Api.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

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

        var searchQuery = $"title:{normalizedQuery} OR author:{normalizedQuery}";
        var encodedQuery = Uri.EscapeDataString(searchQuery);

        // Pasiimam daugiau, tada savo pusėje paliekam max 30 tikrų matchų
        var url = $"search.json?q={encodedQuery}&limit=100";

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
            .Select(book => new BookSearchResultDto
            {
                ExternalId = book.Key ?? "",
                Title = book.Title ?? "",
                Author = book.AuthorName != null
                    ? string.Join(", ", book.AuthorName)
                    : "",
                CoverUrl = book.CoverI.HasValue
                    ? $"https://covers.openlibrary.org/b/id/{book.CoverI}-M.jpg"
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
