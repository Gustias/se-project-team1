using System.Net;
using System.Net.Http.Json;

namespace BookClub.Api.Tests.IntegrationTests;

[Trait("Category", "Integration")]
public class ReadingLogApiTests : IClassFixture<ApiFactory>
{
    private readonly ApiTestClient _api;

    public ReadingLogApiTests(ApiFactory factory) => _api = new ApiTestClient(factory);

    private record LogView(int Id, int StartPage, int EndPage, int PagesRead, string Date);
    private record DayView(string Date, int PagesRead);
    private record SummaryView(int TotalPages, List<DayView> Days);

    private Task<HttpResponseMessage> LogAsync(int userId, int bookId, int start, int end, string date) =>
        _api.PostAsync("/api/reading-log", new { userId, bookId, startPage = start, endPage = end, date });

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
    public async Task Create_ForMissingBook_Returns400()
    {
        var userId = await _api.RegisterAndLoginAsync();

        using var response = await LogAsync(userId, 999_999, 1, 5, "2026-11-09");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
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
}
