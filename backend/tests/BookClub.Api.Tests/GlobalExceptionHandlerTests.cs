using System.Text.Json;
using BookClub.Api.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace BookClub.Api.Tests;

public class GlobalExceptionHandlerTests
{
    private static GlobalExceptionHandler CreateHandler()
    {
        return new GlobalExceptionHandler(
            NullLogger<GlobalExceptionHandler>.Instance);
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        return context;
    }

    private static async Task<JsonElement> ReadResponseBodyAsync(
        HttpResponse response)
    {
        response.Body.Position = 0;

        using var document =
            await JsonDocument.ParseAsync(response.Body);

        return document.RootElement.Clone();
    }

    [Fact]
    public async Task TryHandleAsync_WhenBookNotFound_Returns404()
    {
        var handler = CreateHandler();
        var context = CreateHttpContext();
        var exception = new BookNotFoundException(42);

        var handled = await handler.TryHandleAsync(
            context,
            exception,
            CancellationToken.None);

        var body = await ReadResponseBodyAsync(context.Response);

        Assert.True(handled);
        Assert.Equal(
            StatusCodes.Status404NotFound,
            context.Response.StatusCode);

        Assert.Equal(
            "Not Found",
            body.GetProperty("title").GetString());

        Assert.Equal(
            "Book with ID 42 was not found.",
            body.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task TryHandleAsync_WhenReadingProgressAlreadyExists_Returns409()
    {
        var handler = CreateHandler();
        var context = CreateHttpContext();
        var exception =
            new ReadingProgressAlreadyExistsException();

        var handled = await handler.TryHandleAsync(
            context,
            exception,
            CancellationToken.None);

        var body = await ReadResponseBodyAsync(context.Response);

        Assert.True(handled);
        Assert.Equal(
            StatusCodes.Status409Conflict,
            context.Response.StatusCode);

        Assert.Equal(
            "Conflict",
            body.GetProperty("title").GetString());

        Assert.Equal(
            "Reading progress for this user and book already exists.",
            body.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task TryHandleAsync_WhenDomainExceptionOccurs_Returns400()
    {
        var handler = CreateHandler();
        var context = CreateHttpContext();
        var exception =
            new TestDomainException("Invalid domain state.");

        var handled = await handler.TryHandleAsync(
            context,
            exception,
            CancellationToken.None);

        var body = await ReadResponseBodyAsync(context.Response);

        Assert.True(handled);
        Assert.Equal(
            StatusCodes.Status400BadRequest,
            context.Response.StatusCode);

        Assert.Equal(
            "Bad Request",
            body.GetProperty("title").GetString());

        Assert.Equal(
            "Invalid domain state.",
            body.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task TryHandleAsync_WhenUnexpectedExceptionOccurs_HidesInternalDetails()
    {
        var handler = CreateHandler();
        var context = CreateHttpContext();

        const string internalMessage =
            "Database password or other internal details.";

        var exception =
            new InvalidOperationException(internalMessage);

        var handled = await handler.TryHandleAsync(
            context,
            exception,
            CancellationToken.None);

        var body = await ReadResponseBodyAsync(context.Response);

        Assert.True(handled);
        Assert.Equal(
            StatusCodes.Status500InternalServerError,
            context.Response.StatusCode);

        Assert.Equal(
            "Internal Server Error",
            body.GetProperty("title").GetString());

        Assert.Equal(
            "An unexpected error occurred.",
            body.GetProperty("detail").GetString());

        Assert.DoesNotContain(
            internalMessage,
            body.ToString());
    }

    private sealed class TestDomainException : DomainException
    {
        public TestDomainException(string message)
            : base(message)
        {
        }
    }
}
