using System.Collections.Concurrent;
using BookClub.Api.DTOs;

namespace BookClub.Api.Services;

public class OpenLibrarySearchCache
{
    private readonly ConcurrentDictionary<string, CacheEntry> _entries = new();

    private readonly ConcurrentDictionary<
        string,
        Lazy<Task<List<BookSearchResultDto>>>> _inFlight = new();

    private readonly TimeSpan _timeToLive;

    public OpenLibrarySearchCache(TimeSpan timeToLive)
    {
        _timeToLive = timeToLive;
    }

    public bool TryGet(
        string query,
        out List<BookSearchResultDto> results)
    {
        var key = NormalizeKey(query);

        if (_entries.TryGetValue(key, out var entry))
        {
            if (entry.ExpiresAt > DateTimeOffset.UtcNow)
            {
                results = new List<BookSearchResultDto>(entry.Results);
                return true;
            }

            _entries.TryRemove(key, out _);
        }

        results = new List<BookSearchResultDto>();
        return false;
    }

    public void Set(
        string query,
        List<BookSearchResultDto> results)
    {
        var key = NormalizeKey(query);

        var entry = new CacheEntry(
            new List<BookSearchResultDto>(results),
            DateTimeOffset.UtcNow.Add(_timeToLive));

        _entries[key] = entry;
    }

    public async Task<List<BookSearchResultDto>> GetOrCreateAsync(
        string query,
        Func<Task<List<BookSearchResultDto>>> factory,
        CancellationToken cancellationToken = default)
    {
        if (TryGet(query, out var cachedResults))
        {
            return cachedResults;
        }

        var key = NormalizeKey(query);

        var newLazy = new Lazy<Task<List<BookSearchResultDto>>>(
            () => CreateAndCacheAsync(query, factory),
            LazyThreadSafetyMode.ExecutionAndPublication);

        var lazy = _inFlight.GetOrAdd(key, newLazy);

        if (ReferenceEquals(lazy, newLazy))
        {
            _ = lazy.Value.ContinueWith(
                completedTask =>
                {
                    if (_inFlight.TryGetValue(key, out var current)
                        && ReferenceEquals(current, lazy))
                    {
                        _inFlight.TryRemove(key, out _);
                    }
                },
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
        }

        var results = await lazy.Value.WaitAsync(cancellationToken);

        return new List<BookSearchResultDto>(results);
    }

    private async Task<List<BookSearchResultDto>> CreateAndCacheAsync(
        string query,
        Func<Task<List<BookSearchResultDto>>> factory)
    {
        // Another request may have completed between the initial cache check
        // and the creation of this in-flight operation.
        if (TryGet(query, out var cachedResults))
        {
            return cachedResults;
        }

        var results = await factory();

        Set(query, results);

        return results;
    }

    private static string NormalizeKey(string query)
    {
        return string.Join(
                " ",
                query.Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries))
            .ToLowerInvariant();
    }

    private sealed record CacheEntry(
        List<BookSearchResultDto> Results,
        DateTimeOffset ExpiresAt);
}