using System.Net;
using System.Net.Http.Json;
using BookClub.Api.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BookClub.Api.Tests.IntegrationTests;

public class BooksApiTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public BooksApiTests(ApiFactory factory)
    {
        _client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });
    }

    private async Task<HttpResponseMessage> PostWithCsrfAsync( // create custom POST to send requests with CSRF token
        string url,
        object body)
    {
        // Obtain CSRF token and antiforgery cookie.
        var csrf = await _client.GetFromJsonAsync<CsrfTokenResponse>(
            "/api/auth/csrf");

        Assert.NotNull(csrf);

        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(body)
        };

        request.Headers.Add("X-CSRF-TOKEN", csrf.Token);

        return await _client.SendAsync(request);
    }

    private class CsrfTokenResponse
    {
        public string Token { get; set; } = "";
    }

    [Fact]
    public async Task Create_ThenGet_ReturnsTheSameBook()
    {
        var body = new
        {
            externalId = Guid.NewGuid().ToString(),
            title = "Dune",
            author = "Frank Herbert"
        };

        using var created = await PostWithCsrfAsync(
            "/api/books", body);

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.NotNull(created.Headers.Location);

        var fetched = await _client.GetFromJsonAsync<GetBookDto>(
            created.Headers.Location);

        Assert.NotNull(fetched);
        Assert.Equal("Dune", fetched.Title);
        Assert.Equal("Frank Herbert", fetched.Author);
    }

    [Fact]
    public async Task Create_WhenTitleIsEmpty_Returns400()
    {
        var body = new
        {
            externalId = "x",
            title = "",
            author = "Frank Herbert"
        };

        using var response = await PostWithCsrfAsync(
            "/api/books", body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
