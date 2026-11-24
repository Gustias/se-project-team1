using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Mvc;

namespace BookClub.Api.Tests.IntegrationTests;

[Trait("Category", "Integration")]
public class AuthApiTests : IClassFixture<ApiFactory>
{
    private const string Password = "Test-Passw0rd!";
    private readonly HttpClient _client;

    public AuthApiTests(ApiFactory factory) =>
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

    private record CsrfToken(string Token);

    private static string NewEmail() => $"{Guid.NewGuid():N}@example.com";

    private async Task<HttpResponseMessage> PostAsync(
        string url, object? body = null, bool withCsrf = true)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        if (body is not null) request.Content = JsonContent.Create(body);

        if (withCsrf)
        {
            // fetched fresh every time: the token changes after login/logout
            var csrf = await _client.GetFromJsonAsync<CsrfToken>("/api/auth/csrf");
            request.Headers.Add("X-CSRF-TOKEN", csrf!.Token);
        }

        return await _client.SendAsync(request);
    }

    private async Task<string> RegisterAsync()
    {
        var email = NewEmail();
        using var response = await PostAsync("/api/auth/register",
            new { userName = email, email, password = Password });
        Assert.True(response.IsSuccessStatusCode);
        return email;
    }

    private static async Task AssertValidationErrorAsync(
        HttpResponseMessage response, string expectedField)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content
            .ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.NotNull(problem);
        Assert.Contains(expectedField, problem.Errors.Keys);
    }

    [Fact]
    public async Task Register_Login_Me_Logout_FullJourney()
    {
        var email = await RegisterAsync();

        using var login = await PostAsync("/api/auth/login", new { email, password = Password });
        Assert.True(login.IsSuccessStatusCode);

        using var me = await _client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.OK, me.StatusCode);
        Assert.Contains(email, await me.Content.ReadAsStringAsync());

        using var logout = await PostAsync("/api/auth/logout");
        Assert.True(logout.IsSuccessStatusCode);

        using var meAfter = await _client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, meAfter.StatusCode);
    }

    [Fact]
    public async Task Login_WrongPasswordAndUnknownEmail_LookIdentical()
    {
        var email = await RegisterAsync();

        using var wrongPassword = await PostAsync("/api/auth/login",
            new { email, password = "Wrong-Passw0rd!" });
        using var unknownEmail = await PostAsync("/api/auth/login",
            new { email = NewEmail(), password = Password });

        Assert.Equal(HttpStatusCode.Unauthorized, wrongPassword.StatusCode);
        Assert.Equal(wrongPassword.StatusCode, unknownEmail.StatusCode);
        Assert.Equal(await wrongPassword.Content.ReadAsStringAsync(),
                     await unknownEmail.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Login_AfterTooManyFailures_LocksOutEvenWithCorrectPassword()
    {
        var email = await RegisterAsync();

        for (var i = 0; i < 10; i++)
        {
            using var attempt = await PostAsync("/api/auth/login",
                new { email, password = "Wrong-Passw0rd!" });
        }

        using var locked = await PostAsync("/api/auth/login", new { email, password = Password });
        Assert.False(locked.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Post_WithoutCsrfToken_IsRejected_ButWorksWithOne()
    {
        var email = NewEmail();
        var body = new { userName = email, email, password = Password };

        using var rejected = await PostAsync("/api/auth/register", body, withCsrf: false);
        Assert.Equal(HttpStatusCode.BadRequest, rejected.StatusCode);

        // control case: the exact same request succeeds with a token,
        // so the 400 above really was about CSRF
        using var accepted = await PostAsync("/api/auth/register", body);
        Assert.True(accepted.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Me_WhenNotLoggedIn_Returns401()
    {
        using var response = await _client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WhenEmailIsEmpty_Returns400()
    {
        using var response = await PostAsync("/api/auth/login",
            new
            {
                email = "",
                password = Password
            });

        await AssertValidationErrorAsync(response, "Email");
    }

    [Fact]
    public async Task Login_WhenEmailIsInvalid_Returns400()
    {
        using var response = await PostAsync("/api/auth/login",
            new
            {
                email = "invalid-email",
                password = Password
            });

        await AssertValidationErrorAsync(response, "Email");
    }

    [Fact]
    public async Task Login_WhenPasswordIsEmpty_Returns400()
    {
        using var response = await PostAsync("/api/auth/login",
            new
            {
                email = NewEmail(),
                password = ""
            });

        await AssertValidationErrorAsync(response, "Password");
    }

    [Fact]
    public async Task Logout_WhenNotLoggedIn_Returns401()
    {
        using var response = await PostAsync("/api/auth/logout");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithoutCsrfToken_Returns400()
    {
        var email = await RegisterAsync();
        var body = new { email, password = Password };

        using var rejected = await PostAsync(
            "/api/auth/login", body, withCsrf: false);

        Assert.Equal(HttpStatusCode.BadRequest, rejected.StatusCode);

        using var accepted = await PostAsync(
            "/api/auth/login", body);

        Assert.Equal(HttpStatusCode.OK, accepted.StatusCode);
    }

    [Fact]
    public async Task Login_SessionPersistsAcrossRequests()
    {
        var email = await RegisterAsync();

        using var login = await PostAsync("/api/auth/login",
            new { email, password = Password });

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        for (var i = 0; i < 3; i++)
        {
            using var response = await _client.GetAsync("/api/auth/me");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(email, await response.Content.ReadAsStringAsync());
        }
    }
}
