using BookClub.Api.Data;
using BookClub.Api.Data.Repositories;
using BookClub.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BookClub.Api.Tests;

public class OwnedEntityRepositoryTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static void AddReadingLog(
        AppDbContext context,
        int id,
        int userId)
    {
        context.ReadingLogs.Add(new ReadingLog
        {
            Id = id,
            UserId = userId,
            BookId = 1,
            StartPage = 10,
            EndPage = 20,
            Date = new DateOnly(2026, 11, 24)
        });
    }

    [Fact]
    public async Task GetEntityAsync_WhenEntityBelongsToUser_ReturnsEntity()
    {
        using var context = CreateContext();

        AddReadingLog(context, 1, 1);
        await context.SaveChangesAsync();

        var repository =
            new OwnedEntityRepository<ReadingLog>(context);

        var result = await repository.GetEntityAsync(1, 1);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal(1, result.UserId);
    }

    [Fact]
    public async Task GetEntityAsync_WhenEntityBelongsToAnotherUser_ReturnsNull()
    {
        using var context = CreateContext();

        AddReadingLog(context, 1, 2);
        await context.SaveChangesAsync();

        var repository =
            new OwnedEntityRepository<ReadingLog>(context);

        var result = await repository.GetEntityAsync(1, 1);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetMappedAsync_MapsEntityToRequestedResultType()
    {
        using var context = CreateContext();

        AddReadingLog(context, 1, 1);
        await context.SaveChangesAsync();

        var repository =
            new OwnedEntityRepository<ReadingLog>(context);

        var result = await repository.GetMappedAsync(
            1,
            1,
            log => log.EndPage - log.StartPage);

        Assert.Equal(10, result);
    }

    [Fact]
    public async Task DeleteAsync_WhenEntityBelongsToUser_RemovesEntity()
    {
        using var context = CreateContext();

        AddReadingLog(context, 1, 1);
        await context.SaveChangesAsync();

        var repository =
            new OwnedEntityRepository<ReadingLog>(context);

        var result = await repository.DeleteAsync(1, 1);

        Assert.True(result);
        Assert.Empty(context.ReadingLogs);
    }

    [Fact]
    public async Task DeleteAsync_WhenEntityDoesNotBelongToUser_ReturnsFalse()
    {
        using var context = CreateContext();

        AddReadingLog(context, 1, 2);
        await context.SaveChangesAsync();

        var repository =
            new OwnedEntityRepository<ReadingLog>(context);

        var result = await repository.DeleteAsync(1, 1);

        Assert.False(result);
        Assert.Single(context.ReadingLogs);
    }
}