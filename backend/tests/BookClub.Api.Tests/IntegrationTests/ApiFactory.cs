using BookClub.Api.Data;
using BookClub.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace BookClub.Api.Tests.IntegrationTests;

public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _db =
        new PostgreSqlBuilder("postgres:16").Build(); // start a throwaway container to replicate real world production behavior

    async Task IAsyncLifetime.InitializeAsync()
    {
        await _db.StartAsync();

        using var scope = Services.CreateScope();

        scope.ServiceProvider
            .GetRequiredService<AppDbContext>()
            .Database.Migrate();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting(
            "ConnectionStrings:ConnectionString",
            _db.GetConnectionString());

        builder.ConfigureTestServices(services =>
        {
            services.AddHttpClient<OpenLibraryService>()
                .ConfigurePrimaryHttpMessageHandler(
                    () => new FakeOpenLibraryHandler());
        });
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _db.DisposeAsync();
    }
}
