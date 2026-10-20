using BookClub.Api.Data;
using BookClub.Api.DTOs;
using BookClub.Api.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql.Internal;
namespace BookClub.Api.Services;

public class BookRatingService
{
    private readonly AppDbContext _dbContext;

    public BookRatingService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<BookRatingDto?> CreateAsync(
        int userId,
        int bookId,
        int rating)

    {
        var userExists = await _dbContext.Users
            .AnyAsync(u => u.Id == userId);

        if (!userExists)
        {
            return null;
        }

        var bookExists = await _dbContext.Books
            .AnyAsync(b => b.Id == bookId);

        if (!bookExists)
        {
            return null;
        }

        var hasReadBook = await _dbContext.ReadingProgresses
            .AnyAsync(rp =>
                rp.UserId == userId &&
                rp.BookId == bookId &&
                rp.Status == ReadingStatus.Read);

        if (!hasReadBook)
        {
            return null;
        }

        var existing = await _dbContext.BookRatingEntries
            .FirstOrDefaultAsync(r =>
                r.UserId == userId &&
                r.BookId == bookId);

        if (existing is not null)
        {
            return null;
        }

        var bookRating = new BookRating(rating);

        var entry = new BookRatingEntry
        {
            UserId = userId,
            BookId = bookId,
            Rating = bookRating
        };

        _dbContext.BookRatingEntries.Add(entry);
        await _dbContext.SaveChangesAsync();

        return ToDto(entry);
    }

    public async Task<BookRatingDto?> GetAsync(int id)
    {
        var entry = await _dbContext.BookRatingEntries
            .FirstOrDefaultAsync(r => r.Id == id);

        if (entry is null)
        {
            return null;
        }

        return ToDto(entry);
    }

    private static BookRatingDto ToDto(BookRatingEntry entry)
    {
        return new BookRatingDto
        {
            Id = entry.Id,
            UserId = entry.UserId,
            BookId = entry.BookId,
            Rating = entry.Rating.Value
        };
    }
}