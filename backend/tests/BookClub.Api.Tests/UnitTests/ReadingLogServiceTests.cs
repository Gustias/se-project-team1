using BookClub.Api.Data;
using BookClub.Api.Models;
using BookClub.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace BookClub.Api.Tests;

public class ReadingLogServiceTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        return new AppDbContext(options);
    }

    private static void CreateMockBookEntry(AppDbContext context)
    {
        context.Books.Add(new Book {
            Id = 1,
            ExternalId = "externalId",
            Author = null,
            Title = "TestTitle",
        });
    }

    private static void CreateMockUserEntry(AppDbContext context)
    {
        context.Users.Add(new User {
            Id = 1,
            UserName = "TestUser",
        });
    }

    private static void AddReadingLog(AppDbContext context, int userId, int startPage, int endPage, DateOnly date)
    {
        context.ReadingLogs.Add(new ReadingLog
        {
            UserId = userId,
            BookId = 1,
            StartPage = startPage,
            EndPage = endPage,
            Date = date
        });
    }

    [Fact]
    public async Task CreateAsync_WhenUserDoesNotExist_ReturnsNull()
    {
        using var context = CreateContext();
        CreateMockBookEntry(context);
        context.SaveChanges();

        var service = new ReadingLogService(context);
        var result = await service.CreateAsync(1, 1, 10, 11, new DateOnly(2026,11,09));

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_WhenBookDoesNotExist_ReturnsNull()
    {
        using var context = CreateContext();
        CreateMockUserEntry(context);
        context.SaveChanges();

        var service = new ReadingLogService(context);
        var result = await service.CreateAsync(1, 1, 10, 11, new DateOnly(2026,11,09));

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_WhenAllConditionsAreMet_ReturnsDto()
    {
        using var context = CreateContext();
        CreateMockUserEntry(context);
        CreateMockBookEntry(context);
        context.SaveChanges();

        var service = new ReadingLogService(context);
        var result = await service.CreateAsync(1, 1, 10, 11, new DateOnly(2026,11,09));

        Assert.Single(context.ReadingLogs);
        Assert.NotNull(result);
        Assert.Equal(10, result.StartPage);
        Assert.Equal(11, result.EndPage);
        Assert.Equal(new DateOnly(2026, 11, 09), result.Date);
    }

    [Fact]
    public async Task GetAsync_WhenEntryDoesNotExist_ReturnsNull()
    {
        using var context = CreateContext();

        var service = new ReadingLogService(context);
        var result = await service.GetAsync(1);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAsync_WhenEntryExists_ReturnsDto()
    {
        using var context = CreateContext();
        context.ReadingLogs.Add(new ReadingLog
        {
            Id = 1,
            BookId = 1,
            UserId = 1,
            StartPage = 10,
            EndPage = 11,
            Date = new DateOnly(2026,11,09)
        });
        context.SaveChanges();

        var service = new ReadingLogService(context);
        var result = await service.GetAsync(1);

        Assert.NotNull(result);
        Assert.Equal(10, result.StartPage);
        Assert.Equal(11, result.EndPage);
        Assert.Equal(new DateOnly(2026, 11, 09), result.Date);
    }

    [Fact]
    public async Task GetSummaryAsync_WhenNoLogsInRange_ReturnsEmptySummary()
    {
        using var context = CreateContext();
        var from = new DateOnly(2026, 11, 01);
        var to = new DateOnly(2026, 11, 30);

        var service = new ReadingLogService(context);
        var result = await service.GetSummaryAsync(1, from, to);

        Assert.Equal(from, result.From);
        Assert.Equal(to, result.To);
        Assert.Equal(0, result.TotalPages);
        Assert.Empty(result.Days);
    }

    [Fact]
    public async Task GetSummaryAsync_WhenMultipleLogsOnSameDay_GroupsAndSumsPages()
    {
        using var context = CreateContext();
        var date = new DateOnly(2026, 11, 09);
        AddReadingLog(context, 1, 10, 20, date); // 10 pages
        AddReadingLog(context, 1, 30, 35, date); // 5 pages
        context.SaveChanges();

        var service = new ReadingLogService(context);
        var result = await service.GetSummaryAsync(1, date, date);

        var day = Assert.Single(result.Days);
        Assert.Equal(date, day.Date);
        Assert.Equal(15, day.PagesRead);
        Assert.Equal(15, result.TotalPages);
    }

    [Fact]
    public async Task GetSummaryAsync_WhenLogsBelongToAnotherUser_ExcludesThem()
    {
        using var context = CreateContext();
        var date = new DateOnly(2026, 11, 09);
        AddReadingLog(context, 1, 0, 10, date);
        AddReadingLog(context, 2, 0, 50, date); // other user
        context.SaveChanges();

        var service = new ReadingLogService(context);
        var result = await service.GetSummaryAsync(1, date, date);

        Assert.Equal(10, result.TotalPages);
    }


    [Fact]
    public async Task DeleteAsync_WhenEntryDoesNotExist_ReturnsFalse()
    {
        using var context = CreateContext();

        var service = new ReadingLogService(context);
        var result = await service.DeleteAsync(1);

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_WhenEntryExists_ReturnsTrue()
    {
        using var context = CreateContext();
        context.ReadingLogs.Add(new ReadingLog
        {
            Id = 1,
            BookId = 1,
            UserId = 1,
            StartPage = 10,
            EndPage = 11,
            Date = new DateOnly(2026, 11, 09)
        });
        context.SaveChanges();

        var service = new ReadingLogService(context);
        var result = await service.DeleteAsync(1);

        Assert.True(result);
        Assert.Empty(context.ReadingLogs);
    }
}
