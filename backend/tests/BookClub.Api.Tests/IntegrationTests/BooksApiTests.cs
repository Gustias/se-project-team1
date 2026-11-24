using System.Net;
using System.Net.Http.Json;
using BookClub.Api.DTOs;

namespace BookClub.Api.Tests.IntegrationTests;

[Trait("Category", "Integration")]
public class BooksApiTests : IClassFixture<ApiFactory>
{
    private readonly ApiTestClient _api;

    public BooksApiTests(ApiFactory factory) => _api = new ApiTestClient(factory);

    [Fact]
    public async Task Create_ThenGet_ReturnsTheSameBook()
    {
        await _api.RegisterAndLoginAsync();

        var body = new
        {
            externalId = Guid.NewGuid().ToString(),
            title = "Dune",
            author = "Frank Herbert"
        };

        using var created = await _api.PostAsync("/api/books", body);

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.NotNull(created.Headers.Location);

        var fetched = await _api.Http.GetFromJsonAsync<GetBookDto>(created.Headers.Location);

        Assert.NotNull(fetched);
        Assert.Equal("Dune", fetched.Title);
        Assert.Equal("Frank Herbert", fetched.Author);
    }

    [Fact]
    public async Task Create_WhenTitleIsEmpty_Returns400_BecauseOfTitleValidation()
    {
        await _api.RegisterAndLoginAsync();

        var body = new
        {
            externalId = Guid.NewGuid().ToString(),
            title = "",
            author = "Frank Herbert"
        };

        using var response = await _api.PostAsync("/api/books", body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("title", content, StringComparison.OrdinalIgnoreCase);
    }
}
