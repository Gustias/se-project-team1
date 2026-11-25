using BookClub.Api.Data;
using BookClub.Api.DTOs;
using BookClub.Api.Models;
using BookClub.Api.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace BookClub.Api.Services;

public class ReadingProgressService
{
    private readonly AppDbContext _dbContext;

    public ReadingProgressService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetReadingProgressDto> CreateAsync(
        int userId,
        int bookId,
        int progress,
        string? lastChapterRead,
        ReadingStatus status,
        int? totalPages = null,
        int? pagesRead = null)
    {
        var book = await _dbContext.Books.FindAsync(bookId);

        if (book is null)
        {
            throw new BookNotFoundException(bookId);
        }

        var existing = await _dbContext.ReadingProgresses
            .FirstOrDefaultAsync(rp =>
                rp.UserId == userId &&
                rp.BookId == bookId);

        if (existing is not null)
        {
            throw new ReadingProgressAlreadyExistsException();
        }

        var entry = new ReadingProgress
        {
            UserId = userId,
            BookId = bookId,
            Progress = progress,
            LastChapterRead = lastChapterRead,
            DateRead = DateOnly.FromDateTime(DateTime.UtcNow),
            Status = status,
            TotalPages = totalPages,
            PagesRead = pagesRead ?? 0
        };

        _dbContext.ReadingProgresses.Add(entry);
        await _dbContext.SaveChangesAsync();

        return ToDto(entry);
    }

    public async Task<GetReadingProgressDto?> UpdateAsync(
        int id,
        int userId,
        int progress,
        String? lastChapterRead,
        ReadingStatus status,
        int? totalPages = null,
        int? pagesRead = null)
    {
        var existing = await _dbContext.ReadingProgresses
            .FirstOrDefaultAsync(rp =>
                rp.Id == id &&
                rp.UserId == userId
            );

        if (existing is null)
        {
            return null;
        }

        existing.Progress = progress;
        existing.LastChapterRead = lastChapterRead;
        existing.Status = status;

        if (totalPages is not null)
        {
            existing.TotalPages = totalPages.Value;
        } 

        // Omitted pagesRead keeps the stored value.
        if (pagesRead is not null)
        {
            existing.PagesRead = pagesRead.Value;
        }

        await _dbContext.SaveChangesAsync();

        return ToDto(existing);
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        var existing = await _dbContext.ReadingProgresses
            .FirstOrDefaultAsync(rp =>
                rp.Id == id &&
                rp.UserId == userId);

        if (existing is null)
        {
            return false;
        }

        _dbContext.ReadingProgresses.Remove(existing);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<GetReadingProgressDto?> GetAsync(int id, int userId)
    {
        var existing = await _dbContext.ReadingProgresses
        .FirstOrDefaultAsync(rp =>
            rp.Id == id &&
            rp.UserId == userId);

        if (existing is null)
        {
            return null;
        }

        return ToDto(existing);
    }

    public async Task<GetReadingProgressDto?> GetByWorkAsync(
        string workExternalId,
        int userId)
    {
        // Books may be stored as "OL123W" or "/works/OL123W".
        var prefixed = $"/works/{workExternalId}";

        var existing = await _dbContext.ReadingProgresses
            .FirstOrDefaultAsync(rp =>
                rp.UserId == userId &&
                (rp.Book.ExternalId == workExternalId ||
                 rp.Book.ExternalId == prefixed));

        return existing is null ? null : ToDto(existing);
    }

    private static GetReadingProgressDto ToDto(ReadingProgress entry)
    {
        return new GetReadingProgressDto
        {
            Id = entry.Id,
            UserId = entry.UserId,
            BookId = entry.BookId,
            Progress = entry.Progress,
            LastChapterRead = entry.LastChapterRead,
            DateRead = entry.DateRead,
            Status = entry.Status,
            TotalPages = entry.TotalPages,
            PagesRead = entry.PagesRead
        };
    }
}
