using System.Net;
using System.Net.Http.Json;

namespace BookClub.Api.Tests.IntegrationTests;

[Trait("Category", "Integration")]
public class ReadingProgressApiTests : IClassFixture<ApiFactory>
{
    private const int Reading = 0, Read = 1, WantToRead = 2;
    private readonly ApiTestClient _api;
    private readonly ApiFactory _factory;

    public ReadingProgressApiTests(ApiFactory factory)
    {
        _factory = factory;
        _api = new ApiTestClient(factory);
    }

    private record ProgressView(int Id, int UserId, int BookId, int Progress, string? LastChapterRead, int Status);
    private record LibraryItemView(int Id, int BookId, int Status);

    private async Task<(int OwnerId, int BookId, int Id, ApiTestClient OtherApi)>
    CreateEntryWithTwoUsersAsync()
    {
        var ownerId = await _api.RegisterAndLoginAsync();
        var bookId = await _api.CreateBookAsync();
        var id = await _api.CreateProgressAsync(ownerId, bookId, Reading);

        var otherApi = new ApiTestClient(_factory);
        var otherUserId = await otherApi.RegisterAndLoginAsync();

        Assert.NotEqual(ownerId, otherUserId);

        return (ownerId, bookId, id, otherApi);
    }

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

    [Fact]
    public async Task GetLibrary_ReturnsEntriesWithDifferentStatuses()
    {
        var userId = await _api.RegisterAndLoginAsync();

        var readingBookId = await _api.CreateBookAsync();
        var readBookId = await _api.CreateBookAsync();
        var wantToReadBookId = await _api.CreateBookAsync();

        var readingId = await _api.CreateProgressAsync(
            userId,
            readingBookId,
            Reading);

        var readId = await _api.CreateProgressAsync(
            userId,
            readBookId,
            Read);

        var wantToReadId = await _api.CreateProgressAsync(
            userId,
            wantToReadBookId,
            WantToRead);

        var entries = await _api.Http
            .GetFromJsonAsync<List<LibraryItemView>>(
                "/api/reading-progress");

        Assert.NotNull(entries);
        Assert.Contains(entries, entry =>
            entry.Id == readingId &&
            entry.Status == Reading);

        Assert.Contains(entries, entry =>
            entry.Id == readId &&
            entry.Status == Read);

        Assert.Contains(entries, entry =>
            entry.Id == wantToReadId &&
            entry.Status == WantToRead);
    }

    [Fact]
    public async Task GetLibrary_WithStatusFilter_ReturnsOnlyMatchingEntries()
    {
        var userId = await _api.RegisterAndLoginAsync();

        var readingBookId = await _api.CreateBookAsync();
        var wantToReadBookId = await _api.CreateBookAsync();

        await _api.CreateProgressAsync(
            userId,
            readingBookId,
            Reading);

        var wantToReadId = await _api.CreateProgressAsync(
            userId,
            wantToReadBookId,
            WantToRead);

        var entries = await _api.Http
            .GetFromJsonAsync<List<LibraryItemView>>(
                $"/api/reading-progress?status={WantToRead}");

        Assert.NotNull(entries);

        Assert.Single(entries);

        Assert.Equal(
            wantToReadId,
            entries[0].Id);

        Assert.Equal(
            WantToRead,
            entries[0].Status);
    }

    [Fact]
    public async Task GetLibrary_DoesNotReturnAnotherUsersEntries()
    {
        var userId = await _api.RegisterAndLoginAsync();

        var ownBookId = await _api.CreateBookAsync();

        var ownEntryId = await _api.CreateProgressAsync(
            userId,
            ownBookId,
            WantToRead);

        var otherApi = new ApiTestClient(_factory);
        var otherUserId = await otherApi.RegisterAndLoginAsync();

        var otherBookId = await otherApi.CreateBookAsync();

        var otherEntryId = await otherApi.CreateProgressAsync(
            otherUserId,
            otherBookId,
            WantToRead);

        var entries = await _api.Http
            .GetFromJsonAsync<List<LibraryItemView>>(
                "/api/reading-progress");

        Assert.NotNull(entries);

        Assert.Contains(entries, entry =>
            entry.Id == ownEntryId);

        Assert.DoesNotContain(entries, entry =>
            entry.Id == otherEntryId);
    }

    [Fact]
    public async Task Get_WhenNotLoggedIn_Returns401()
    {
        using var response = await _api.Http.GetAsync(
            "/api/reading-progress/1");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_WhenNotLoggedIn_Returns401()
    {
        using var response = await _api.PostAsync(
            "/api/reading-progress",
            new
            {
                bookId = 1,
                progress = 42,
                status = Reading
            });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_WhenEntryBelongsToAnotherUser_Returns404()
    {
        var (_, _, id, otherApi) = await CreateEntryWithTwoUsersAsync();

        using var response = await otherApi.Http.GetAsync(
            $"/api/reading-progress/{id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        using var ownerResponse = await _api.Http.GetAsync(
            $"/api/reading-progress/{id}");

        Assert.Equal(HttpStatusCode.OK, ownerResponse.StatusCode);
    }

    [Fact]
    public async Task Update_WhenEntryBelongsToAnotherUser_Returns404()
    {
        var (ownerId, _, id, otherApi) = await CreateEntryWithTwoUsersAsync();

        using var response = await otherApi.PutAsync(
            $"/api/reading-progress/{id}",
            new
            {
                progress = 100,
                lastChapterRead = "Final chapter",
                status = Read
            });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var fetched = await _api.Http.GetFromJsonAsync<ProgressView>(
            $"/api/reading-progress/{id}");

        Assert.NotNull(fetched);
        Assert.Equal(ownerId, fetched.UserId);
        Assert.Equal(0, fetched.Progress);
        Assert.Equal(Reading, fetched.Status);
        Assert.Null(fetched.LastChapterRead);
    }

    [Fact]
    public async Task Delete_WhenEntryBelongsToAnotherUser_Returns404()
    {
        var (ownerId, bookId, id, otherApi) =
            await CreateEntryWithTwoUsersAsync();

        using var response = await otherApi.DeleteAsync(
            $"/api/reading-progress/{id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var fetched = await _api.Http.GetFromJsonAsync<ProgressView>(
            $"/api/reading-progress/{id}");

        Assert.NotNull(fetched);
        Assert.Equal(id, fetched.Id);
        Assert.Equal(ownerId, fetched.UserId);
        Assert.Equal(bookId, fetched.BookId);
    }

    [Fact]
    public async Task Create_WhenAnotherUserIdIsProvided_UsesAuthenticatedUserId()
    {
        var userId = await _api.RegisterAndLoginAsync();
        var bookId = await _api.CreateBookAsync();

        var otherApi = new ApiTestClient(_factory);
        var otherUserId = await otherApi.RegisterAndLoginAsync();

        Assert.NotEqual(userId, otherUserId);

        using var response = await _api.PostAsync(
            "/api/reading-progress",
            new
            {
                userId = otherUserId,
                bookId,
                progress = 42,
                status = Reading
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content
            .ReadFromJsonAsync<ProgressView>();

        Assert.NotNull(created);
        Assert.Equal(userId, created.UserId);
        Assert.NotEqual(otherUserId, created.UserId);

        var fetched = await _api.Http.GetFromJsonAsync<ProgressView>(
            $"/api/reading-progress/{created.Id}");

        Assert.NotNull(fetched);
        Assert.Equal(userId, fetched.UserId);

        using var otherResponse = await otherApi.Http.GetAsync(
            $"/api/reading-progress/{created.Id}");

        Assert.Equal(HttpStatusCode.NotFound, otherResponse.StatusCode);
    }
}
