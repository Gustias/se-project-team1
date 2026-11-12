using BookClub.Api.Data;
using BookClub.Api.DTOs;
using BookClub.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BookClub.Api.Services;

public class BookService
{
    private readonly AppDbContext _dbContext;
    public BookService(AppDbContext dbContext, HttpClient httpClient)
    {
        _dbContext = dbContext;
    }

    public async Task<CreateBookResult> CreateAsync(
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
            return new CreateBookResult(ToDto(existing), false);
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

        return new CreateBookResult(ToDto(entry), true);

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
