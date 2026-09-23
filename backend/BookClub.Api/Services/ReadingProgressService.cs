using BookClub.Api.Data;
using BookClub.Api.DTOs;
using BookClub.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BookClub.Api.Services;

public class ReadingProgressService
{
    private readonly AppDbContext _dbContext;

    public ReadingProgressService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetReadingProgressDto?> CreateAsync(
        int userId,
        int bookId,
        int progress,
        int? chapter)
    {
        var existing = await _dbContext.ReadingProgresses
            .FirstOrDefaultAsync(rp =>
                rp.UserId == userId &&
                rp.BookId == bookId);

        if (existing is not null)
        {
            return null;
        }

        var entry = new ReadingProgress
        {
            UserId = userId,
            BookId = bookId,
            Progress = progress,
            Chapter = chapter,
            DateRead = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        _dbContext.ReadingProgresses.Add(entry);
        await _dbContext.SaveChangesAsync();

        return ToDto(entry);
    }

    public async Task<GetReadingProgressDto?> UpdateAsync(
        int id,
        int progress,
        int? chapter)
    {
        var existing = await _dbContext.ReadingProgresses.FindAsync(id);

        if (existing is null)
        {
            return null;
        }

        existing.Progress = progress;
        existing.Chapter = chapter;

        await _dbContext.SaveChangesAsync();

        return ToDto(existing);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _dbContext.ReadingProgresses.FindAsync(id);

        if (existing is null)
        {
            return false;
        }

        _dbContext.ReadingProgresses.Remove(existing);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<GetReadingProgressDto?> GetAsync(int id)
    {
        var existing = await _dbContext.ReadingProgresses.FindAsync(id);

        if (existing is null)
        {
            return null;
        }

        return ToDto(existing);
    }

    private static GetReadingProgressDto ToDto(ReadingProgress entry)
    {
        return new GetReadingProgressDto
        {
            Id = entry.Id,
            UserId = entry.UserId,
            BookId = entry.BookId,
            Progress = entry.Progress,
            Chapter = entry.Chapter,
            DateRead = entry.DateRead
        };
    }
}