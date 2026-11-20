using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BookClub.Api.Tests.IntegrationTests;

public class ApiTestClient // helper class
{
    private const string Password = "Test-Passw0rd!";
    public HttpClient Http { get; }

    public ApiTestClient(ApiFactory factory) =>
        Http = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

    private record CsrfToken(string Token);
    private record IdOnly(int Id);

    public Task<HttpResponseMessage> PostAsync(string url, object? body = null) => SendAsync(HttpMethod.Post, url, body);
    public Task<HttpResponseMessage> PutAsync(string url, object? body = null) => SendAsync(HttpMethod.Put, url, body);
    public Task<HttpResponseMessage> DeleteAsync(string url) => SendAsync(HttpMethod.Delete, url, null);

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, object? body)
    {
        var csrf = await Http.GetFromJsonAsync<CsrfToken>("/api/auth/csrf");
        using var request = new HttpRequestMessage(method, url);
        if (body is not null) request.Content = JsonContent.Create(body);
        request.Headers.Add("X-CSRF-TOKEN", csrf!.Token);
        return await Http.SendAsync(request);
    }

    public async Task<int> RegisterAndLoginAsync()
    {
        var email = $"{Guid.NewGuid():N}@example.com";
        using var register = await PostAsync("/api/auth/register", new { userName = email, email, password = Password });
        register.EnsureSuccessStatusCode();
        using var login = await PostAsync("/api/auth/login", new { email, password = Password });
        login.EnsureSuccessStatusCode();
        return (await Http.GetFromJsonAsync<IdOnly>("/api/auth/me"))!.Id;
    }

    public async Task<int> CreateBookAsync()
    {
        using var response = await PostAsync("/api/books",
            new { externalId = Guid.NewGuid().ToString(), title = "Dune", author = "Frank Herbert" });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<IdOnly>())!.Id;
    }

    public async Task<int> CreateProgressAsync(int userId, int bookId, int status)
    {
        using var response = await PostAsync("/api/reading-progress",
            new { userId, bookId, progress = 0, status });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<IdOnly>())!.Id;
    }
}
