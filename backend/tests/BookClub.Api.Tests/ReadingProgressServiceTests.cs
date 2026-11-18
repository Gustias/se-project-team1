using BookClub.Api.Data;
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

    private static void CreateMockBookEntry(AppDbContext context)
    {
        context.Books.Add(new Book {
            Id = 1,
            ExternalId = "externalId",
            Author = null,
            Title = "TestTitle",
        });
    }

    private static void AddReadingProgress(AppDbContext context, int id, int userId, ReadingStatus status)
    {
        context.ReadingProgresses.Add(new ReadingProgress {
            Id = id,
            UserId = userId,
            BookId = 1,
            Progress = 67,
            LastChapterRead = "Chapter 1",
            DateRead = new DateOnly(2026, 11, 09),
            Status = status
        });
    }

    [Fact]
    public async Task CreateAsync_WhenBookDoesNotExist_ReturnsNull()
    {
        using var context = CreateContext();

        var service = new ReadingProgressService(context);
        var result = await service.CreateAsync(1, 1, 10, "Chapter 1", ReadingStatus.Reading);

        Assert.Null(result);
        Assert.Empty(context.ReadingProgresses);
    }

    [Fact]
    public async Task CreateAsync_WhenUserAlreadyHasProgressForBook_ReturnsNull()
    {
        using var context = CreateContext();
        CreateMockBookEntry(context);
        AddReadingProgress(context, 1, 1, ReadingStatus.Reading);
        context.SaveChanges();

        var service = new ReadingProgressService(context);
        var result = await service.CreateAsync(1, 1, 50, "Chapter 5", ReadingStatus.Read);

        Assert.Null(result);
        Assert.Single(context.ReadingProgresses);
    }

    [Fact]
    public async Task CreateAsync_WhenAnotherUserHasProgressForSameBook_ReturnsDto()
    {
        using var context = CreateContext();
        CreateMockBookEntry(context);
        AddReadingProgress(context, 1, 2, ReadingStatus.Reading); // other user
        context.SaveChanges();

        var service = new ReadingProgressService(context);
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

        var service = new ReadingProgressService(context);
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

        var service = new ReadingProgressService(context);
        var result = await service.UpdateAsync(1, 50, "Chapter 5", ReadingStatus.Read);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_WhenEntryExists_UpdatesAndReturnsDto()
    {
        using var context = CreateContext();
        AddReadingProgress(context, 1, 1, ReadingStatus.Reading);
        context.SaveChanges();

        var service = new ReadingProgressService(context);
        var result = await service.UpdateAsync(1, 100, "Final chapter", ReadingStatus.Read);

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

        var service = new ReadingProgressService(context);
        var result = await service.DeleteAsync(1);

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_WhenEntryExists_ReturnsTrue()
    {
        using var context = CreateContext();
        AddReadingProgress(context, 1, 1, ReadingStatus.Reading);
        context.SaveChanges();

        var service = new ReadingProgressService(context);
        var result = await service.DeleteAsync(1);

        Assert.True(result);
        Assert.Empty(context.ReadingProgresses);
    }

    [Fact]
    public async Task GetAsync_WhenEntryDoesNotExist_ReturnsNull()
    {
        using var context = CreateContext();

        var service = new ReadingProgressService(context);
        var result = await service.GetAsync(1);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAsync_WhenEntryExists_ReturnsDto()
    {
        using var context = CreateContext();
        AddReadingProgress(context, 1, 1, ReadingStatus.Reading);
        context.SaveChanges();

        var service = new ReadingProgressService(context);
        var result = await service.GetAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal(1, result.UserId);
        Assert.Equal(1, result.BookId);
        Assert.Equal(67, result.Progress);
        Assert.Equal("Chapter 1", result.LastChapterRead);
        Assert.Equal(new DateOnly(2026, 11, 09), result.DateRead);
        Assert.Equal(ReadingStatus.Reading, result.Status);
    }


}
