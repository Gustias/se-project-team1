using BookClub.Api.DTOs;
using BookClub.Api.Services;

namespace BookClub.Api.Tests;

public class OpenLibrarySearchCacheTests
{
    private static List<BookSearchResultDto> CreateResults()
    {
        return new List<BookSearchResultDto>
        {
            new()
            {
                ExternalId = "/works/OL123W",
                Title = "Dune",
                Author = "Frank Herbert"
            }
        };
    }

    [Fact]
    public async Task GetOrCreateAsync_WhenResultIsCached_RunsFactoryOnlyOnce()
    {
        var cache = new OpenLibrarySearchCache(TimeSpan.FromMinutes(15));
        var factoryCalls = 0;

        async Task<List<BookSearchResultDto>> Factory()
        {
            Interlocked.Increment(ref factoryCalls);
            await Task.Yield();
            return CreateResults();
        }

        var first = await cache.GetOrCreateAsync("Dune", Factory);
        var second = await cache.GetOrCreateAsync("Dune", Factory);

        Assert.Equal(1, factoryCalls);
        Assert.Single(first);
        Assert.Single(second);
        Assert.Equal("Dune", second[0].Title);
    }

    [Fact]
    public async Task GetOrCreateAsync_NormalizesEquivalentQueries_UsesSameCacheEntry()
    {
        var cache = new OpenLibrarySearchCache(TimeSpan.FromMinutes(15));
        var factoryCalls = 0;

        async Task<List<BookSearchResultDto>> Factory()
        {
            Interlocked.Increment(ref factoryCalls);
            await Task.Yield();
            return CreateResults();
        }

        await cache.GetOrCreateAsync("Harry Potter", Factory);
        await cache.GetOrCreateAsync("  harry   potter  ", Factory);
        await cache.GetOrCreateAsync("HARRY POTTER", Factory);

        Assert.Equal(1, factoryCalls);
    }

    [Fact]
    public async Task GetOrCreateAsync_WhenEntryExpires_RunsFactoryAgain()
    {
        var cache = new OpenLibrarySearchCache(TimeSpan.Zero);
        var factoryCalls = 0;

        async Task<List<BookSearchResultDto>> Factory()
        {
            Interlocked.Increment(ref factoryCalls);
            await Task.Yield();
            return CreateResults();
        }

        await cache.GetOrCreateAsync("Dune", Factory);
        await cache.GetOrCreateAsync("Dune", Factory);

        Assert.Equal(2, factoryCalls);
    }

    [Fact]
    public async Task GetOrCreateAsync_WhenRequestsAreConcurrent_RunsFactoryOnlyOnce()
    {
        var cache = new OpenLibrarySearchCache(TimeSpan.FromMinutes(15));
        var factoryCalls = 0;

        async Task<List<BookSearchResultDto>> Factory()
        {
            Interlocked.Increment(ref factoryCalls);

            await Task.Delay(100);

            return CreateResults();
        }

        var tasks = Enumerable.Range(0, 20)
            .Select(_ => cache.GetOrCreateAsync("Dune", Factory))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        Assert.Equal(1, factoryCalls);
        Assert.Equal(20, results.Length);

        foreach (var result in results)
        {
            Assert.Single(result);
            Assert.Equal("Dune", result[0].Title);
        }
    }
}
