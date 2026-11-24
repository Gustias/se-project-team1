using BookClub.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BookClub.Api.Data.Repositories;

public class OwnedEntityRepository<TEntity>
    where TEntity : class, IUserOwnedEntity
{
    private readonly AppDbContext _dbContext;

    public OwnedEntityRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TEntity?> GetEntityAsync(
        int id,
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<TEntity>()
            .FirstOrDefaultAsync(
                entity =>
                    entity.Id == id &&
                    entity.UserId == userId,
                cancellationToken);
    }

    public async Task<TResult?> GetMappedAsync<TResult>(
        int id,
        int userId,
        Func<TEntity, TResult> mapper,
        CancellationToken cancellationToken = default)
    {
        var entity = await GetEntityAsync(
            id,
            userId,
            cancellationToken);

        if (entity is null)
        {
            return default;
        }

        return mapper(entity);
    }

    public async Task<bool> DeleteAsync(
        int id,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var entity = await GetEntityAsync(
            id,
            userId,
            cancellationToken);

        if (entity is null)
        {
            return false;
        }

        _dbContext.Set<TEntity>().Remove(entity);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return true;
    }
}