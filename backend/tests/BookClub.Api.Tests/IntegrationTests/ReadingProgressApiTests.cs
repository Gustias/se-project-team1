using System.Net;
using System.Net.Http.Json;

namespace BookClub.Api.Tests.IntegrationTests;

[Trait("Category", "Integration")]
public class ReadingProgressApiTests : IClassFixture<ApiFactory>
{
    private const int Reading = 0, Read = 1; // enum values sent as numbers
    private readonly ApiTestClient _api;

    public ReadingProgressApiTests(ApiFactory factory) => _api = new ApiTestClient(factory);

    private record ProgressView(int Id, int UserId, int BookId, int Progress, string? LastChapterRead, int Status);

    [Fact]
    public async Task Create_ThenGet_ReturnsSameEntry()
    {
        var userId = await _api.RegisterAndLoginAsync();
        var bookId = await _api.CreateBookAsync();

        using var created = await _api.PostAsync("/api/reading-progress",
            new { userId, bookId, progress = 42, lastChapterRead = "Chapter 4", status = Reading });

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var fetched = await _api.Http.GetFromJsonAsync<ProgressView>(created.Headers.Location);
        Assert.Equal(42, fetched!.Progress);
        Assert.Equal("Chapter 4", fetched.LastChapterRead);
        Assert.Equal(Reading, fetched.Status);
    }

    [Fact]
    public async Task Create_Twice_ForSameUserAndBook_Returns409()
    {
        var userId = await _api.RegisterAndLoginAsync();
        var bookId = await _api.CreateBookAsync();
        var body = new { userId, bookId, progress = 0, status = Reading };

        using var first = await _api.PostAsync("/api/reading-progress", body);
        using var second = await _api.PostAsync("/api/reading-progress", body);

        Assert.Equal(HttpStatusCode.Created, first.StatusCode); // control case
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Create_ForMissingBook_Returns409()
    {
        var userId = await _api.RegisterAndLoginAsync();

        using var response = await _api.PostAsync("/api/reading-progress",
            new { userId, bookId = 999999, progress = 0, status = Reading });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithProgressAbove100_Returns400()
    {
        var userId = await _api.RegisterAndLoginAsync();
        var bookId = await _api.CreateBookAsync();

        using var response = await _api.PostAsync("/api/reading-progress",
            new { userId, bookId, progress = 101, status = Reading });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_ChangesStatusAndProgress()
    {
        var userId = await _api.RegisterAndLoginAsync();
        var bookId = await _api.CreateBookAsync();
        var id = await _api.CreateProgressAsync(userId, bookId, Reading);

        using var response = await _api.PutAsync($"/api/reading-progress/{id}",
            new { progress = 100, lastChapterRead = "Final chapter", status = Read });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var fetched = await _api.Http.GetFromJsonAsync<ProgressView>($"/api/reading-progress/{id}");
        Assert.Equal(Read, fetched!.Status);
        Assert.Equal(100, fetched.Progress);
    }

    [Fact]
    public async Task Delete_RemovesTheEntry()
    {
        var userId = await _api.RegisterAndLoginAsync();
        var bookId = await _api.CreateBookAsync();
        var id = await _api.CreateProgressAsync(userId, bookId, Reading);

        using var deleted = await _api.DeleteAsync($"/api/reading-progress/{id}");
        using var after = await _api.Http.GetAsync($"/api/reading-progress/{id}");

        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, after.StatusCode);
    }
}
