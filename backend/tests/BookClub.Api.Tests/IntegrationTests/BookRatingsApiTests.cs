using System.Net;
using System.Net.Http.Json;

namespace BookClub.Api.Tests.IntegrationTests;

[Trait("Category", "Integration")]
public class BookRatingsApiTests : IClassFixture<ApiFactory>
{
    private const int Reading = 0, Read = 1;
    private readonly ApiTestClient _api;
    private readonly ApiFactory _factory;

    public BookRatingsApiTests(ApiFactory factory)
    {
        _factory = factory;
        _api = new ApiTestClient(factory);
    }

    private record RatingView(int Id, int UserId, int BookId, int Rating);

    private async Task<(ApiTestClient OtherApi, int OtherUserId)>
        CreateOtherUserAsync(int userId)
    {
        var otherApi = new ApiTestClient(_factory);
        var otherUserId = await otherApi.RegisterAndLoginAsync();

        Assert.NotEqual(userId, otherUserId);

        return (otherApi, otherUserId);
    }

    [Fact]
    public async Task Rate_FinishedBook_ReturnsTheRating()
    {
        var userId = await _api.RegisterAndLoginAsync();
        var bookId = await _api.CreateBookAsync();
        await _api.CreateProgressAsync(userId, bookId, Read);

        using var response = await _api.PostAsync("/api/book-ratings", new { userId, bookId, rating = 5 });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var rating = await response.Content.ReadFromJsonAsync<RatingView>();
        Assert.Equal(5, rating!.Rating);

        var fetched = await _api.Http.GetFromJsonAsync<RatingView>($"/api/book-ratings/{rating.Id}");
        Assert.Equal(bookId, fetched!.BookId);
    }

    [Fact]
    public async Task Rate_BookThatIsNotFinished_Returns409()
    {
        var userId = await _api.RegisterAndLoginAsync();
        var bookId = await _api.CreateBookAsync();
        await _api.CreateProgressAsync(userId, bookId, Reading);

        using var response = await _api.PostAsync("/api/book-ratings", new { userId, bookId, rating = 4 });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Rate_SameBookTwice_Returns409()
    {
        var userId = await _api.RegisterAndLoginAsync();
        var bookId = await _api.CreateBookAsync();
        await _api.CreateProgressAsync(userId, bookId, Read);
        var body = new { userId, bookId, rating = 3 };

        using var first = await _api.PostAsync("/api/book-ratings", body);
        using var second = await _api.PostAsync("/api/book-ratings", body);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode); // control case
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public async Task Rate_OutOfRange_Returns400(int rating)
    {
        var userId = await _api.RegisterAndLoginAsync();
        var bookId = await _api.CreateBookAsync();
        await _api.CreateProgressAsync(userId, bookId, Read);

        using var response = await _api.PostAsync("/api/book-ratings", new { userId, bookId, rating });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Get_UnknownId_Returns404()
    {
        await _api.RegisterAndLoginAsync();

        using var response = await _api.Http.GetAsync("/api/book-ratings/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_WhenNotLoggedIn_Returns401()
    {
        using var response = await _api.Http.GetAsync(
            "/api/book-ratings/1");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Rate_WhenNotLoggedIn_Returns401()
    {
        using var response = await _api.PostAsync(
            "/api/book-ratings",
            new
            {
                bookId = 1,
                rating = 5
            });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_WhenEntryBelongsToAnotherUser_Returns404()
    {
        var ownerId = await _api.RegisterAndLoginAsync();
        var bookId = await _api.CreateBookAsync();

        await _api.CreateProgressAsync(ownerId, bookId, Read);

        using var created = await _api.PostAsync(
            "/api/book-ratings",
            new { bookId, rating = 5 });

        Assert.Equal(HttpStatusCode.OK, created.StatusCode);

        var rating = await created.Content.ReadFromJsonAsync<RatingView>();

        Assert.NotNull(rating);
        Assert.Equal(ownerId, rating.UserId);

        var (otherApi, _) = await CreateOtherUserAsync(ownerId);

        using var response = await otherApi.Http.GetAsync(
            $"/api/book-ratings/{rating.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        using var ownerResponse = await _api.Http.GetAsync(
            $"/api/book-ratings/{rating.Id}");

        Assert.Equal(HttpStatusCode.OK, ownerResponse.StatusCode);
    }

    [Fact]
    public async Task Rate_WhenOnlyAnotherUserHasReadBook_Returns409()
    {
        var userId = await _api.RegisterAndLoginAsync();
        var bookId = await _api.CreateBookAsync();

        var (otherApi, otherUserId) = await CreateOtherUserAsync(userId);

        await otherApi.CreateProgressAsync(
            otherUserId, bookId, Read);

        using var response = await _api.PostAsync(
            "/api/book-ratings",
            new { bookId, rating = 5 });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Rate_WhenAnotherUserHasRatedSameBook_ReturnsOk()
    {
        var userId = await _api.RegisterAndLoginAsync();
        var bookId = await _api.CreateBookAsync();

        await _api.CreateProgressAsync(userId, bookId, Read);

        var (otherApi, otherUserId) = await CreateOtherUserAsync(userId);

        await otherApi.CreateProgressAsync(otherUserId, bookId, Read);

        using var otherResponse = await otherApi.PostAsync(
            "/api/book-ratings",
            new { bookId, rating = 5 });

        Assert.Equal(HttpStatusCode.OK, otherResponse.StatusCode);

        var otherRating = await otherResponse.Content
            .ReadFromJsonAsync<RatingView>();

        Assert.NotNull(otherRating);
        Assert.Equal(otherUserId, otherRating.UserId);

        using var response = await _api.PostAsync(
            "/api/book-ratings",
            new { bookId, rating = 4 });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var rating = await response.Content
            .ReadFromJsonAsync<RatingView>();

        Assert.NotNull(rating);
        Assert.Equal(userId, rating.UserId);
        Assert.Equal(bookId, rating.BookId);
        Assert.Equal(4, rating.Rating);
        Assert.NotEqual(otherRating.Id, rating.Id);

        var fetched = await otherApi.Http.GetFromJsonAsync<RatingView>(
            $"/api/book-ratings/{otherRating.Id}");

        Assert.NotNull(fetched);
        Assert.Equal(otherUserId, fetched.UserId);
        Assert.Equal(bookId, fetched.BookId);
        Assert.Equal(5, fetched.Rating);
    }

    [Fact]
    public async Task Rate_WhenAnotherUserIdIsProvided_UsesAuthenticatedUserId()
    {
        var userId = await _api.RegisterAndLoginAsync();
        var bookId = await _api.CreateBookAsync();

        await _api.CreateProgressAsync(userId, bookId, Read);

        var (otherApi, otherUserId) = await CreateOtherUserAsync(userId);

        using var response = await _api.PostAsync(
            "/api/book-ratings",
            new
            {
                userId = otherUserId,
                bookId,
                rating = 5
            });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var created = await response.Content
            .ReadFromJsonAsync<RatingView>();

        Assert.NotNull(created);
        Assert.Equal(userId, created.UserId);
        Assert.NotEqual(otherUserId, created.UserId);

        var fetched = await _api.Http.GetFromJsonAsync<RatingView>(
            $"/api/book-ratings/{created.Id}");

        Assert.NotNull(fetched);
        Assert.Equal(userId, fetched.UserId);

        using var otherResponse = await otherApi.Http.GetAsync(
            $"/api/book-ratings/{created.Id}");

        Assert.Equal(HttpStatusCode.NotFound, otherResponse.StatusCode);
    }
}
