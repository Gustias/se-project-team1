using BookClub.Api.Data;
using BookClub.Api.Data.Repositories;
using BookClub.Api.Exceptions;
using BookClub.Api.Models;
using BookClub.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace BookClub.Api.Tests;

public class ReadingProgressServiceTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        return new AppDbContext(options);
    }

    private static ReadingProgressService CreateService(
        AppDbContext context)
    {
        var repository =
            new OwnedEntityRepository<ReadingProgress>(context);

        return new ReadingProgressService(
            context,
            repository);
    }

    private static void CreateMockBookEntry(AppDbContext context)
    {
        context.Books.Add(new Book
        {
            Id = 1,
            ExternalId = "externalId",
            Author = null,
            Title = "TestTitle",
        });
    }

    private static void AddReadingProgress(
        AppDbContext context,
        int id,
        int userId,
        ReadingStatus status,
        int pagesRead = 0)
    {
        context.ReadingProgresses.Add(new ReadingProgress
        {
            Id = id,
            UserId = userId,
            BookId = 1,
            Progress = 67,
            LastChapterRead = "Chapter 1",
            DateRead = new DateOnly(2026, 11, 09),
            Status = status,
            PagesRead = pagesRead
        });
    }

    [Fact]
    public async Task UpdateAsync_WhenPagesIncrease_CreatesReadingLog()
    {
        using var context = CreateContext();

        AddReadingProgress(
            context,
            1,
            1,
            ReadingStatus.Reading,
            pagesRead: 50);

        await context.SaveChangesAsync();

        var service = CreateService(context);

        await service.UpdateAsync(
            1,
            1,
            38,
            "Chapter 5",
            ReadingStatus.Reading,
            totalPages: 200,
            pagesRead: 75);

        var log = Assert.Single(context.ReadingLogs);

        Assert.Equal(1, log.UserId);
        Assert.Equal(1, log.BookId);
        Assert.Equal(50, log.StartPage);
        Assert.Equal(75, log.EndPage);
        Assert.Equal(25, log.EndPage - log.StartPage);
    }

    [Theory]
    [InlineData(50)]
    [InlineData(40)]
    public async Task UpdateAsync_WhenPagesDoNotIncrease_DoesNotCreateReadingLog(
        int newPagesRead)
    {
        using var context = CreateContext();

        AddReadingProgress(
            context,
            1,
            1,
            ReadingStatus.Reading,
            pagesRead: 50);

        await context.SaveChangesAsync();

        var service = CreateService(context);

        await service.UpdateAsync(
            1,
            1,
            25,
            "Chapter 4",
            ReadingStatus.Reading,
            totalPages: 200,
            pagesRead: newPagesRead);

        Assert.Empty(context.ReadingLogs);
    }

    [Fact]
    public async Task CreateAsync_WhenBookDoesNotExist_ThrowsBookNotFoundException()
    {
        using var context = CreateContext();

        var service = CreateService(context);

        await Assert.ThrowsAsync<BookNotFoundException>(() =>
            service.CreateAsync(
                1,
                1,
                10,
                "Chapter 1",
                ReadingStatus.Reading));

        Assert.Empty(context.ReadingProgresses);
    }

    [Fact]
    public async Task CreateAsync_WhenUserAlreadyHasProgressForBook_ThrowsReadingProgressAlreadyExistsException()
    {
        using var context = CreateContext();
        CreateMockBookEntry(context);
        AddReadingProgress(
            context,
            1,
            1,
            ReadingStatus.Reading);

        await context.SaveChangesAsync();

        var service = CreateService(context);

        await Assert.ThrowsAsync<ReadingProgressAlreadyExistsException>(() =>
            service.CreateAsync(
                1,
                1,
                50,
                "Chapter 5",
                ReadingStatus.Read));

        Assert.Single(context.ReadingProgresses);
    }

    [Fact]
    public async Task CreateAsync_WhenAnotherUserHasProgressForSameBook_ReturnsDto()
    {
        using var context = CreateContext();
        CreateMockBookEntry(context);
        AddReadingProgress(context, 1, 2, ReadingStatus.Reading); // other user
        context.SaveChanges();

        var service = CreateService(context);
        var result = await service.CreateAsync(1, 1, 10, "Chapter 1", ReadingStatus.Reading);

        Assert.NotNull(result);
        Assert.Equal(2, context.ReadingProgresses.Count());
    }

    [Fact]
    public async Task CreateAsync_WhenAllConditionsAreMet_SavesAndReturnsDto()
    {
        using var context = CreateContext();
        CreateMockBookEntry(context);
        context.SaveChanges();

        var service = CreateService(context);
        var result = await service.CreateAsync(1, 1, 42, "Chapter 4", ReadingStatus.Reading);

        Assert.NotNull(result);
        Assert.Equal(1, result.UserId);
        Assert.Equal(1, result.BookId);
        Assert.Equal(42, result.Progress);
        Assert.Equal("Chapter 4", result.LastChapterRead);
        Assert.Equal(ReadingStatus.Reading, result.Status);
        Assert.Single(context.ReadingProgresses);
    }

    [Fact]
    public async Task UpdateAsync_WhenEntryDoesNotExist_ReturnsNull()
    {
        using var context = CreateContext();

        var service = CreateService(context);
        var result = await service.UpdateAsync(1, 1, 50, "Chapter 5", ReadingStatus.Read);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_WhenEntryExists_UpdatesAndReturnsDto()
    {
        using var context = CreateContext();
        AddReadingProgress(context, 1, 1, ReadingStatus.Reading);
        context.SaveChanges();

        var service = CreateService(context);
        var result = await service.UpdateAsync(1, 1, 100, "Final chapter", ReadingStatus.Read);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal(100, result.Progress);
        Assert.Equal("Final chapter", result.LastChapterRead);
        Assert.Equal(ReadingStatus.Read, result.Status);

        // Check database too
        var saved = context.ReadingProgresses.Single();
        Assert.Equal(100, saved.Progress);
        Assert.Equal("Final chapter", saved.LastChapterRead);
        Assert.Equal(ReadingStatus.Read, saved.Status);
    }

    [Fact]
    public async Task DeleteAsync_WhenEntryDoesNotExist_ReturnsFalse()
    {
        using var context = CreateContext();

        var service = CreateService(context);
        var result = await service.DeleteAsync(1, 1);

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_WhenEntryExists_ReturnsTrue()
    {
        using var context = CreateContext();
        AddReadingProgress(context, 1, 1, ReadingStatus.Reading);
        context.SaveChanges();

        var service = CreateService(context);
        var result = await service.DeleteAsync(1, 1);

        Assert.True(result);
        Assert.Empty(context.ReadingProgresses);
    }

    [Fact]
    public async Task GetAsync_WhenEntryDoesNotExist_ReturnsNull()
    {
        using var context = CreateContext();

        var service = CreateService(context);
        var result = await service.GetAsync(1, 1);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAsync_WhenEntryExists_ReturnsDto()
    {
        using var context = CreateContext();
        AddReadingProgress(context, 1, 1, ReadingStatus.Reading);
        context.SaveChanges();

        var service = CreateService(context);
        var result = await service.GetAsync(1, 1);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal(1, result.UserId);
        Assert.Equal(1, result.BookId);
        Assert.Equal(67, result.Progress);
        Assert.Equal("Chapter 1", result.LastChapterRead);
        Assert.Equal(new DateOnly(2026, 11, 09), result.DateRead);
        Assert.Equal(ReadingStatus.Reading, result.Status);
    }

    [Fact]
    public async Task GetAsync_WhenEntryBelongsToAnotherUser_ReturnsNull()
    {
        using var context = CreateContext();

        AddReadingProgress(context, 10, 2, ReadingStatus.Reading);

        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.GetAsync(id: 10, userId: 1);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_WhenEntryBelongsToAnotherUser_ReturnsNull()
    {
        using var context = CreateContext();

        AddReadingProgress(context, 10, 2, ReadingStatus.Reading);

        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.UpdateAsync(
            id: 10,
            userId: 1,
            progress: 100,
            lastChapterRead: "Final chapter",
            status: ReadingStatus.Read);

        Assert.Null(result);

        var saved = await context.ReadingProgresses
            .AsNoTracking()
            .SingleAsync();

        Assert.Equal(10, saved.Id);
        Assert.Equal(2, saved.UserId);
        Assert.Equal(67, saved.Progress);
        Assert.Equal("Chapter 1", saved.LastChapterRead);
        Assert.Equal(ReadingStatus.Reading, saved.Status);
    }

    [Fact]
    public async Task DeleteAsync_WhenEntryBelongsToAnotherUser_ReturnsFalse()
    {
        using var context = CreateContext();

        AddReadingProgress(context, 10, 2, ReadingStatus.Reading);

        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.DeleteAsync(id: 10, userId: 1);

        Assert.False(result);

        var saved = await context.ReadingProgresses
            .AsNoTracking()
            .SingleAsync();

        Assert.Equal(10, saved.Id);
        Assert.Equal(2, saved.UserId);
    }
}
