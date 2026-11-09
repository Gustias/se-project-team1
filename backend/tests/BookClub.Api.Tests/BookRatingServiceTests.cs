using BookClub.Api.Data;
using BookClub.Api.Models;
using BookClub.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace BookClub.Api.Tests;

public class BookRatingServiceTests
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

    private static void CreateMockReadingProgressEntry(AppDbContext context, ReadingStatus readingStatus)
    {
        context.ReadingProgresses.Add(new ReadingProgress {
            Id = 1,
            BookId = 1,
            UserId = 1,
            Status = readingStatus
        });
    }

    [Fact]
    public async Task CreateAsync_WhenUserDoesNotExist_ReturnsNull()
    {
        using var context = CreateContext();
        var service = new BookRatingService(context);

        var result = await service.CreateAsync(1, 1, 5);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_WhenBookDoesNotExist_ReturnsNull()
    {
        using var context = CreateContext();
        CreateMockUserEntry(context);
        await context.SaveChangesAsync();

        var service = new BookRatingService(context);

        var result = await service.CreateAsync(1, 1, 5);
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_WhenTryingToRateABookThatHasNotBeenRead_ReturnsNull()
    {
        using var context = CreateContext();
        CreateMockUserEntry(context);
        CreateMockBookEntry(context);
        CreateMockReadingProgressEntry(context,  ReadingStatus.Reading);
        await context.SaveChangesAsync();

        var service = new BookRatingService(context);

        var result = await service.CreateAsync(1, 1, 5);
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_WhenTryingToRateABookWhichAlreadyHasRating_ReturnsNull()
    {
        using var context = CreateContext();
        CreateMockUserEntry(context);
        CreateMockBookEntry(context);
        CreateMockReadingProgressEntry(context, ReadingStatus.Read);
        context.BookRatingEntries.Add(new BookRatingEntry
        {
            Id = 1,
            BookId = 1,
            UserId = 1,
            Rating = new BookRating(5)
        });
        await context.SaveChangesAsync();

        var service = new BookRatingService(context);

        var result = await service.CreateAsync(1, 1, 5);
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_WhenAllConditionsAreMet_ReturnsDTO()
    {
        using var context = CreateContext();
        CreateMockUserEntry(context);
        CreateMockBookEntry(context);
        CreateMockReadingProgressEntry(context, ReadingStatus.Read);
        await context.SaveChangesAsync();

        var service = new BookRatingService(context);

        var result = await service.CreateAsync(1, 1, 5);
        Assert.NotNull(result);
        Assert.Equal(5, result.Rating);
        Assert.Equal(1, result.BookId);
        Assert.Equal(1, result.UserId);
        Assert.Single(context.BookRatingEntries);

    }

    [Fact]
    public async Task GetAsync_WhenEntryDoesNotExist_ReturnsNull()
    {
        using var context = CreateContext();

        var service = new BookRatingService(context);

        var result = await service.GetAsync(1);
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAsync_WhenEntryExists_ReturnsDto()
    {
        using var context = CreateContext();
        context.BookRatingEntries.Add(new BookRatingEntry
        {
            Id = 1,
            BookId = 1,
            UserId = 1,
            Rating = new BookRating(5)
        });
        await context.SaveChangesAsync();

        var service = new BookRatingService(context);

        var result = await service.GetAsync(1);
        Assert.NotNull(result);
        Assert.Equal(5, result.Rating);
        Assert.Equal(1, result.BookId);
        Assert.Equal(1, result.UserId);
    }


}
