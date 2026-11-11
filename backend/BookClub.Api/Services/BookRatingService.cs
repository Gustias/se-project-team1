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

    public async Task<BookRatingDto?> CreateBookAsync(
        int userId,
        int bookId,
        int rating,
        string? review)
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

        var entry = new BookRatingEntry
        {
        UserId = userId,
        BookId = bookId,
        Rating = rating,
        Review = review,
        CreatedAt = DateTime.UtcNow
        };

        _dbContext.BookRatingEntries.Add(entry);

        await _dbContext.SaveChangesAsync();

        return ToDto(entry);
  }

  public async Task<BookRatingDto?> CreateEditionAsync(
    int userId,
    int bookEditionId,
    int rating,
    string? review)
    {
    //ValidateRating(rating);

    var userExists = await _dbContext.Users
        .AnyAsync(u => u.Id == userId);

    if (!userExists)
    {
      return null;
    }

    var edition = await _dbContext.BookRatingEntries
        .FirstOrDefaultAsync(e => e.Id == bookEditionId);

    if (edition is null)
    {
      return null;
    }

    var hasReadBook = await _dbContext.ReadingProgresses
        .AnyAsync(rp =>
            rp.UserId == userId &&
            rp.BookId == edition.BookId && // need to fix to edition
            rp.Status == ReadingStatus.Read);

    if (!hasReadBook)
    {
      return null;
    }

    var existing = await _dbContext.BookRatingEntries
        .AnyAsync(r =>
            r.UserId == userId &&
            r.BookEditionId == bookEditionId);

    if (existing)
        return null;

    var entry = new BookRatingEntry
    {
        BookId = edition.BookId,
        UserId = userId,
        BookEditionId = bookEditionId,
        Rating = rating,
        Review = review,
        CreatedAt = DateTime.UtcNow
    };

    _dbContext.BookRatingEntries.Add(entry);
    await _dbContext.SaveChangesAsync();

    return ToDto(entry);
}

    public async Task<BookRatingDto?> GetBookAsync(int id)
    {
        var entry = await _dbContext.BookRatingEntries
            .FirstOrDefaultAsync(r => r.Id == id);

        if (entry is null)
        {
            return null;
        }

        return ToDto(entry);
    }

    public async Task<BookRatingDto?> GetEditionAsync(int editionId)
    {
        var entry = await _dbContext.BookRatingEntries
            .FirstOrDefaultAsync(r => r.BookEditionId == editionId);

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
            BookEditionId = entry.BookEditionId,
            Rating = entry.Rating,
            Review = entry.Review,
            CreatedAt = entry.CreatedAt
        };
    }
}