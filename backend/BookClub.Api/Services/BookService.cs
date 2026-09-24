using BookClub.Api.Data;
using BookClub.Api.DTOs;
using BookClub.Api.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

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

    public async Task<GetBookDto?> CreateAsync(
        string externalId,
        string? isbn,
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
            Isbn = isbn,
            Title = title,
            Author = author
        };

        _dbContext.Books.Add(entry);
        await _dbContext.SaveChangesAsync();

        return ToDto(entry); // change later
        
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
            Isbn = entry.Isbn,
            Title = entry.Title,
            Author = entry.Author
        };
    }


}