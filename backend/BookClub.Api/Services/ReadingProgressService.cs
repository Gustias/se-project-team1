using BookClub.Api.Models;

namespace BookClub.Api.Services;

public class ReadingProgressService
{
    public async Task<ReadingProgress> CreateAsync(int userId, int bookId,
        int progress, int? chapter)
    {
        return null;
    }
    
    public async Task<ReadingProgress> UpdateAsync(int id,
        int progress, int? chapter)
    {
        return null;
    }

    public async Task<ReadingProgress> DeleteAsync(int id)
    {
        return null;
    }

    public async Task<ReadingProgress> GetAsync(int id)
    {
        return null;
    }
}