using BookClub.Api.Data;
using BookClub.Api.DTOs;
using BookClub.Api.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

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

    public async Task<CreateBookDto?> CreateAsync(
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

        return something; // change later
        
    }

    public async Task<CreateBookDto?> DeleteAsync(int id)
    {
        
    }
    
    public async Task<CreateBookDto?> GetAsync(int id)
    {
        
    }


}