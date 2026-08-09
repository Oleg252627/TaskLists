using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TaskLists.Application.Abstractions.Persistence;
using TaskLists.Domain.Common;
using TaskLists.Infrastructure.Persistence;

namespace TaskLists.Infrastructure.Repositories;

public class Repository<TEntity>(
    TaskListsDbContext dbContext)
    : IRepository<TEntity>
    where TEntity : Entity
{
    protected TaskListsDbContext DbContext { get; } = dbContext;

    protected DbSet<TEntity> Set { get; } = dbContext.Set<TEntity>();

    public async Task<TEntity?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default,
        params Expression<Func<TEntity, object>>[] includes)
    {
        IQueryable<TEntity> query = Set;

        query = ApplyIncludes(query, includes);

        return await query.SingleOrDefaultAsync(
            x => x.Id == id,
            cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> GetAllAsync(
        CancellationToken cancellationToken = default,
        params Expression<Func<TEntity, object>>[] includes)
    {
        IQueryable<TEntity> query = Set;

        query = ApplyIncludes(query, includes);

        return await query
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        await Set.AddAsync(entity, cancellationToken);
    }

    public void Update(TEntity entity)
    {
        Set.Update(entity);
    }

    public void Remove(TEntity entity)
    {
        Set.Remove(entity);
    }

    private static IQueryable<TEntity> ApplyIncludes(
        IQueryable<TEntity> query,
        IEnumerable<Expression<Func<TEntity, object>>> includes)
    {
        return includes.Aggregate(
            query,
            (current, include) => current.Include(include));
    }
}