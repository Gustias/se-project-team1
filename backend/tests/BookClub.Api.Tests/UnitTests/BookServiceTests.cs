using BookClub.Api.Data;
using BookClub.Api.Models;
using BookClub.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace BookClub.Api.Tests;

public class BookServiceTests //only tests the database related stuff, doesn't touch the http api calls
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        return new AppDbContext(options);
    }

    private static void CreateMockBookEntry(AppDbContext context)
    {
        context.Books.Add(new Book
        {
            Id = 1,
            ExternalId = "externalId",
            Title = "TestTitle",
        });
    }

    [Fact]
    public async Task CreateAsync_WhenBookWithSameExternalIdExists_ReturnsNull()
    {
        using var context = CreateContext();
        CreateMockBookEntry(context);
        context.SaveChanges();

        var service = new BookService(context);
        var result = await service.CreateAsync("externalId", "isbn10", "isbn13", "title", "author");

        Assert.False(result.Created);
    }

    [Fact]
    public async Task CreateAsync_WhenConditionAreMet_ReturnsDto()
    {
        using var context = CreateContext();

        var service = new BookService(context);
        var result = await service.CreateAsync("externalId", "isbn10", "isbn13", "title", "author");

        Assert.Single(context.Books);
        Assert.True(result.Created);
        Assert.Equal("externalId", result.Book.ExternalId);
        Assert.Equal("isbn10", result.Book.Isbn10);
        Assert.Equal("isbn13", result.Book.Isbn13);
        Assert.Equal("title", result.Book.Title);
        Assert.Equal("author", result.Book.Author);
    }

    [Fact]
    public async Task GetAsync_WhenBookDoesNotExist_ReturnsNull()
    {
        using var context = CreateContext();

        var service = new BookService(context);
        var result = await service.GetAsync(1);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAsync_WhenEntryExists_ReturnsDto()
    {
        using var context = CreateContext();
        CreateMockBookEntry(context);
        await context.SaveChangesAsync();

        var service = new BookService(context);
        var result = await service.GetAsync(1);

        Assert.NotNull(result);
        Assert.Equal("externalId", result.ExternalId);
        Assert.Equal("TestTitle", result.Title);
    }
}
