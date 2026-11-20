using System.Net;
using System.Net.Http.Json;

namespace BookClub.Api.Tests.IntegrationTests;

[Trait("Category", "Integration")]
public class BookRatingsApiTests : IClassFixture<ApiFactory>
{
    private const int Reading = 0, Read = 1;
    private readonly ApiTestClient _api;

    public BookRatingsApiTests(ApiFactory factory) => _api = new ApiTestClient(factory);

    private record RatingView(int Id, int UserId, int BookId, int Rating);

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
        using var response = await _api.Http.GetAsync("/api/book-ratings/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
