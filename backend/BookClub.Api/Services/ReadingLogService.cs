using BookClub.Api.Data;
using BookClub.Api.DTOs;
using BookClub.Api.Models;
using Microsoft.EntityFrameworkCore;
using BookClub.Api.Exceptions;
using BookClub.Api.Data.Repositories;

namespace BookClub.Api.Services;

public class ReadingLogService
{
    private readonly AppDbContext _dbContext;
    private readonly OwnedEntityRepository<ReadingLog> _ownedEntityRepository;

    public ReadingLogService(
        AppDbContext dbContext,
        OwnedEntityRepository<ReadingLog> ownedEntityRepository)
    {
        _dbContext = dbContext;
        _ownedEntityRepository = ownedEntityRepository;
    }

    public async Task<ReadingLogDto?> CreateAsync(
        int userId,
        int bookId,
        int startPage,
        int endPage,
        DateOnly date,
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

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(
                u => u.Id == userId,
                cancellationToken);

        if (user is null)
        {
            return null;
        }

        var entry = new ReadingLog
        {
            UserId = userId,
            BookId = bookId,
            StartPage = startPage,
            EndPage = endPage,
            Date = date
        };

        _dbContext.ReadingLogs.Add(entry);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return ToDto(entry);
    }

    public async Task<ReadingLogDto?> GetAsync(
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

    public async Task<ReadingSummaryDto> GetSummaryAsync(
        int userId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default)
    {
        var days = await _dbContext.ReadingLogs
            .Where(l =>
                l.UserId == userId &&
                l.Date >= from &&
                l.Date <= to)
            .GroupBy(l => l.Date)
            .Select(g => new DailyReadingDto
            {
                Date = g.Key,
                PagesRead =
                    g.Sum(l => l.EndPage - l.StartPage)
            })
            .OrderBy(d => d.Date)
            .ToListAsync(cancellationToken);

        var totalBooksRead =
            await _dbContext.ReadingLogs
                .Where(l =>
                    l.UserId == userId &&
                    l.Date >= from &&
                    l.Date <= to)
                .Select(l => l.BookId)
                .Distinct()
                .CountAsync(cancellationToken);

        return new ReadingSummaryDto
        {
            From = from,
            To = to,
            TotalPages =
                days.Sum(d => d.PagesRead),
            TotalBooksRead = totalBooksRead,
            Days = days
        };
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
