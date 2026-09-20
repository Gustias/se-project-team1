using BookClub.Api.Data;
using BookClub.Api.DTOs;
using BookClub.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BookClub.Api.Services;

public class ReadingProgressService
{
    private AppDbContext _dbContext;
    public ReadingProgressService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<ReadingProgress> CreateAsync(int userId, int bookId,
        int progress, int? chapter)
    {

        var existing = await _dbContext.ReadingProgresses
            .FirstOrDefaultAsync(rp => rp.UserId == userId && rp.BookId == bookId);

        if (existing is not null)
        {
            existing.Progress = progress;
            existing.Chapter = chapter;
            await _dbContext.SaveChangesAsync();
            return existing;
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
        
        return entry;
    }
    
    public async Task<ReadingProgress?> UpdateAsync(int id,
        int progress, int? chapter)
    {
        var existing = await _dbContext.ReadingProgresses.FindAsync(id);
        if (existing is null)
        {
            return null;
        }

        existing.Progress = progress;
        existing.Chapter = chapter;
        await _dbContext.SaveChangesAsync();
        return existing;

    }

    public async Task<ReadingProgress?> DeleteAsync(int id)
    {
        var existing = await _dbContext.ReadingProgresses.FindAsync(id);
        if (existing is null)
        {
            return null;
        }

        _dbContext.ReadingProgresses.Remove(existing);
        await _dbContext.SaveChangesAsync();
        return existing;
    }

    public async Task<GetReadingProgressDto?> GetAsync(int id)
    {
        var existing = await _dbContext.ReadingProgresses.FindAsync(id);
        if (existing is null)
        {
            return null;
        }

        GetReadingProgressDto entry = new GetReadingProgressDto
        {
            Id = existing.Id,
            BookId = existing.BookId,
            UserId = existing.UserId,
            Progress = existing.Progress,
            Chapter = existing.Chapter,
            DateRead = existing.DateRead
        };
        
        return entry;
    }
}