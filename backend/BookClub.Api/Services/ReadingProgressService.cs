using BookClub.Api.Data;
using BookClub.Api.Data.Repositories;
using BookClub.Api.DTOs;
using BookClub.Api.Exceptions;
using BookClub.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BookClub.Api.Services;

public class ReadingProgressService
{
    private readonly AppDbContext _dbContext;
    private readonly OwnedEntityRepository<ReadingProgress> _ownedEntityRepository;

    public ReadingProgressService(
        AppDbContext dbContext,
        OwnedEntityRepository<ReadingProgress> ownedEntityRepository)
    {
        _dbContext = dbContext;
        _ownedEntityRepository = ownedEntityRepository;
    }

    public async Task<GetReadingProgressDto> CreateAsync(
        int userId,
        int bookId,
        int progress,
        string? lastChapterRead,
        ReadingStatus status,
        int? totalPages = null,
        int? pagesRead = null,
        CancellationToken cancellationToken = default)
    {
        var book = await _dbContext.Books
            .FirstOrDefaultAsync(
                b => b.Id == bookId,
                cancellationToken);

        if (book is null)
        {
            throw new BookNotFoundException(bookId);
        }

        var existing = await _dbContext.ReadingProgresses
            .FirstOrDefaultAsync(
                rp =>
                    rp.UserId == userId &&
                    rp.BookId == bookId,
                cancellationToken);

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

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return ToDto(entry);
    }

    public async Task<GetReadingProgressDto?> UpdateAsync(
        int id,
        int userId,
        int progress,
        string? lastChapterRead,
        ReadingStatus status,
        int? totalPages = null,
        int? pagesRead = null,
        CancellationToken cancellationToken = default)
    {
        var existing = await _ownedEntityRepository
            .GetEntityAsync(
                id,
                userId,
                cancellationToken);

        if (existing is null)
        {
            return null;
        }

        var previousPagesRead = existing.PagesRead;

        existing.Progress = progress;
        existing.LastChapterRead = lastChapterRead;
        existing.Status = status;

        if (totalPages is not null)
        {
            existing.TotalPages = totalPages.Value;
        }

        if (pagesRead is not null)
        {
            var newPagesRead = pagesRead.Value;

            if (newPagesRead > previousPagesRead)
            {
                _dbContext.ReadingLogs.Add(new ReadingLog
                {
                    UserId = userId,
                    BookId = existing.BookId,
                    StartPage = previousPagesRead,
                    EndPage = newPagesRead,
                    Date = DateOnly.FromDateTime(DateTime.UtcNow)
                });
            }

            existing.PagesRead = newPagesRead;
        }

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return ToDto(existing);
    }

    public async Task<bool> DeleteAsync(
        int id,
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await _ownedEntityRepository.DeleteAsync(
            id,
            userId,
            cancellationToken);
    }

    public async Task<GetReadingProgressDto?> GetAsync(
        int id,
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await _ownedEntityRepository
            .GetMappedAsync(
                id,
                userId,
                ToDto,
                cancellationToken);
    }

    public async Task<GetReadingProgressDto?> GetByWorkAsync(
        string workExternalId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        // Books may be stored as "OL123W" or "/works/OL123W".
        var prefixed = $"/works/{workExternalId}";

        var existing = await _dbContext.ReadingProgresses
            .FirstOrDefaultAsync(
                rp =>
                    rp.UserId == userId &&
                    (
                        rp.Book.ExternalId == workExternalId ||
                        rp.Book.ExternalId == prefixed
                    ),
                cancellationToken);

        return existing is null
            ? null
            : ToDto(existing);
    }

    public async Task<List<ReadingListItemDto>> GetLibraryAsync(
        int userId,
        ReadingStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.ReadingProgresses
            .Where(rp => rp.UserId == userId);

        if (status.HasValue)
        {
            query = query.Where(
                rp => rp.Status == status.Value);
        }

        return await query
            .OrderByDescending(rp => rp.Id)
            .Select(rp => new ReadingListItemDto
            {
                Id = rp.Id,
                BookId = rp.BookId,
                ExternalId = rp.Book.ExternalId,
                Title = rp.Book.Title,
                Author = rp.Book.Author,
                Isbn13 = rp.Book.Isbn13,
                Status = rp.Status,
                Progress = rp.Progress,
                PagesRead = rp.PagesRead,
                TotalPages = rp.TotalPages,
                LastChapterRead = rp.LastChapterRead
            })
            .ToListAsync(cancellationToken);
    }

    private static GetReadingProgressDto ToDto(
        ReadingProgress entry)
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
