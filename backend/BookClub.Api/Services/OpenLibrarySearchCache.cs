using System.Collections.Concurrent;
using BookClub.Api.DTOs;

namespace BookClub.Api.Services;

public class OpenLibrarySearchCache
{
    private readonly ConcurrentDictionary<string, CacheEntry> _entries = new();
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