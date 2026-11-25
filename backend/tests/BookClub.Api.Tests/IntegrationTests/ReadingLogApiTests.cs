using System.Net;
using System.Net.Http.Json;

namespace BookClub.Api.Tests.IntegrationTests;

[Trait("Category", "Integration")]
public class ReadingLogApiTests : IClassFixture<ApiFactory>
{
    private readonly ApiTestClient _api;
    private readonly ApiFactory _factory;

    public ReadingLogApiTests(ApiFactory factory)
    {
        _factory = factory;
        _api = new ApiTestClient(factory);
    }

    private record LogView(int Id, int UserId, int StartPage, int EndPage, int PagesRead, string Date);
    private record DayView(string Date, int PagesRead);
    private record SummaryView(int TotalPages, List<DayView> Days);

    private Task<HttpResponseMessage> LogAsync(int userId, int bookId, int start, int end, string date) =>
        _api.PostAsync("/api/reading-log", new { userId, bookId, startPage = start, endPage = end, date });

    private async Task<(Uri Location, ApiTestClient OtherApi)>
        CreateEntryWithTwoUsersAsync()
    {
        var ownerId = await _api.RegisterAndLoginAsync();
        var bookId = await _api.CreateBookAsync();

        using var created = await LogAsync(
            ownerId, bookId, 10, 20, "2026-11-09");

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        var otherApi = new ApiTestClient(_factory);
        var otherUserId = await otherApi.RegisterAndLoginAsync();

        Assert.NotEqual(ownerId, otherUserId);

        return (created.Headers.Location!, otherApi);
    }

    [Fact]
    public async Task Create_ThenGet_ReturnsSameEntryWithPagesRead()
    {
        var userId = await _api.RegisterAndLoginAsync();
        var bookId = await _api.CreateBookAsync();

        using var created = await LogAsync(userId, bookId, 10, 15, "2026-11-09");

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var fetched = await _api.Http.GetFromJsonAsync<LogView>(created.Headers.Location);
        Assert.Equal(10, fetched!.StartPage);
        Assert.Equal(15, fetched.EndPage);
        Assert.Equal(5, fetched.PagesRead);
    }

    [Fact]
    public async Task Create_WhenEndPageBeforeStartPage_Returns400()
    {
        var userId = await _api.RegisterAndLoginAsync();
        var bookId = await _api.CreateBookAsync();

        using var response = await LogAsync(userId, bookId, 20, 10, "2026-11-09");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_ForMissingBook_Returns404()
    {
        var userId = await _api.RegisterAndLoginAsync();

        using var response = await LogAsync(userId, 999_999, 1, 5, "2026-11-09");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Summary_SumsPagesPerDay_AndTotalsThem()
    {
        var userId = await _api.RegisterAndLoginAsync();
        var bookId = await _api.CreateBookAsync();

        (await LogAsync(userId, bookId, 10, 20, "2026-11-09")).Dispose(); // 10 pages
        (await LogAsync(userId, bookId, 30, 35, "2026-11-09")).Dispose(); // 5 pages, same day
        (await LogAsync(userId, bookId, 40, 50, "2026-11-10")).Dispose(); // 10 pages, next day

        var summary = await _api.Http.GetFromJsonAsync<SummaryView>(
            "/api/reading-log/summary?from=2026-11-01&to=2026-11-30");

        Assert.Equal(25, summary!.TotalPages);
        Assert.Equal(2, summary.Days.Count);
        Assert.Equal(15, summary.Days.Single(d => d.Date == "2026-11-09").PagesRead);
    }

    [Fact]
    public async Task Summary_WhenToIsBeforeFrom_Returns400()
    {
        await _api.RegisterAndLoginAsync();

        using var response = await _api.Http.GetAsync(
            "/api/reading-log/summary?from=2026-11-30&to=2026-11-01");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Delete_RemovesTheEntry()
    {
        var userId = await _api.RegisterAndLoginAsync();
        var bookId = await _api.CreateBookAsync();
        using var created = await LogAsync(userId, bookId, 1, 5, "2026-11-09");

        using var deleted = await _api.DeleteAsync(created.Headers.Location!.AbsolutePath);
        using var after = await _api.Http.GetAsync(created.Headers.Location);

        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, after.StatusCode);
    }

    [Fact]
    public async Task Get_WhenNotLoggedIn_Returns401()
    {
        using var response = await _api.Http.GetAsync(
            "/api/reading-log/1");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_WhenNotLoggedIn_Returns401()
    {
        using var response = await _api.PostAsync(
            "/api/reading-log",
            new
            {
                bookId = 1,
                startPage = 10,
                endPage = 20,
                date = "2026-11-09"
            });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Summary_WhenNotLoggedIn_Returns401()
    {
        using var response = await _api.Http.GetAsync(
            "/api/reading-log/summary?from=2026-11-01&to=2026-11-30");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_WhenEntryBelongsToAnotherUser_Returns404()
    {
        var (location, otherApi) =
            await CreateEntryWithTwoUsersAsync();

        using var response = await otherApi.Http.GetAsync(location);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        using var ownerResponse = await _api.Http.GetAsync(location);

        Assert.Equal(HttpStatusCode.OK, ownerResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_WhenEntryBelongsToAnotherUser_Returns404()
    {
        var (location, otherApi) =
            await CreateEntryWithTwoUsersAsync();

        using var response = await otherApi.DeleteAsync(
            location.AbsolutePath);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var fetched = await _api.Http.GetFromJsonAsync<LogView>(
            location);

        Assert.NotNull(fetched);
        Assert.Equal(10, fetched.StartPage);
        Assert.Equal(20, fetched.EndPage);
        Assert.Equal(10, fetched.PagesRead);
    }

    [Fact]
    public async Task Summary_WhenOtherUserHasEntries_ReturnsOnlyOwnData()
    {
        var userId = await _api.RegisterAndLoginAsync();
        var bookId = await _api.CreateBookAsync();

        using var created = await LogAsync(
            userId, bookId, 10, 20, "2026-11-09");

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        var otherApi = new ApiTestClient(_factory);
        var otherUserId = await otherApi.RegisterAndLoginAsync();

        Assert.NotEqual(userId, otherUserId);

        using var otherCreated = await otherApi.PostAsync(
            "/api/reading-log",
            new
            {
                bookId,
                startPage = 20,
                endPage = 50,
                date = "2026-11-09"
            });

        Assert.Equal(HttpStatusCode.Created, otherCreated.StatusCode);

        const string summaryUrl =
            "/api/reading-log/summary?from=2026-11-01&to=2026-11-30";

        var summary = await _api.Http.GetFromJsonAsync<SummaryView>(
            summaryUrl);

        Assert.NotNull(summary);
        Assert.Equal(10, summary.TotalPages);
        Assert.Single(summary.Days);
        Assert.Equal(10, summary.Days.Single().PagesRead);

        var otherSummary = await otherApi.Http.GetFromJsonAsync<SummaryView>(
            summaryUrl);

        Assert.NotNull(otherSummary);
        Assert.Equal(30, otherSummary.TotalPages);
        Assert.Single(otherSummary.Days);
        Assert.Equal(30, otherSummary.Days.Single().PagesRead);
    }

    [Fact]
    public async Task Create_WhenAnotherUserIdIsProvided_UsesAuthenticatedUserId()
    {
        var userId = await _api.RegisterAndLoginAsync();
        var bookId = await _api.CreateBookAsync();

        var otherApi = new ApiTestClient(_factory);
        var otherUserId = await otherApi.RegisterAndLoginAsync();

        Assert.NotEqual(userId, otherUserId);

        using var response = await LogAsync(
            otherUserId, bookId, 10, 20, "2026-11-09");

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content
            .ReadFromJsonAsync<LogView>();

        Assert.NotNull(created);
        Assert.Equal(userId, created.UserId);
        Assert.NotEqual(otherUserId, created.UserId);

        var fetched = await _api.Http.GetFromJsonAsync<LogView>(
            response.Headers.Location);

        Assert.NotNull(fetched);
        Assert.Equal(userId, fetched.UserId);

        using var otherResponse = await otherApi.Http.GetAsync(
            response.Headers.Location);

        Assert.Equal(HttpStatusCode.NotFound, otherResponse.StatusCode);
    }
}
