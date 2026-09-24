using BookClub.Api.Data;
using BookClub.Api.DTOs;
using BookClub.Api.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace BookClub.Api.Services;
public class BookService
{
    private readonly AppDbContext _dbContext;
    public BookService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public List<BookSearchResultDto> SearchBooks(string query)
    {
        return new List<BookSearchResultDto>
        {
            new BookSearchResultDto
            {
                ExternalId = "test-1",
                Title = "Dune",
                Author = "Frank Herbert",
                CoverUrl = null
            }
        };
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