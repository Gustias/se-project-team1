using BookClub.Api.Data;
using BookClub.Api.DTOs;
using BookClub.Api.Models;

namespace BookClub.Api.Services;

public class ReadingLogService
{
    private readonly AppDbContext _dbContext;

    public ReadingLogService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ReadingLogDto?> CreateAsync(
        int userId,
        int bookId,
        int startPage,
        int endPage,
        DateOnly date
        )
    {

        var book = await _dbContext.Books.FindAsync(bookId);
        if (book is null) return null;
        var user = await _dbContext.Users.FindAsync(userId);
        if (user is null) return null;

        var entry = new ReadingLog
        {
            UserId = userId,
            BookId = bookId,
            StartPage = startPage,
            EndPage = endPage,
            Date = date
        };

        _dbContext.ReadingLogs.Add(entry);
        await _dbContext.SaveChangesAsync();

        return ToDto(entry);
    }

    public async Task<ReadingLogDto?> GetAsync(int id)
    {
        var existing = await _dbContext.ReadingLogs.FindAsync(id);

        if (existing is null)
        {
            return null;
        }

        return ToDto(existing);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _dbContext.ReadingLogs.FindAsync(id);

        if (existing is null)
        {
            return false;
        }

        _dbContext.ReadingLogs.Remove(existing);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    private static ReadingLogDto ToDto(ReadingLog entry)
    {
        return new ReadingLogDto
        {
            Id = entry.Id,
            UserId = entry.UserId,
            BookId = entry.BookId,
            StartPage = entry.StartPage,
            EndPage = entry.EndPage,
            Date = entry.Date
        };
    }
}
