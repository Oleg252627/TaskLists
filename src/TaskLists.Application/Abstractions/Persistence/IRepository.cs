using System.Linq.Expressions;
using TaskLists.Domain.Common;

namespace TaskLists.Application.Abstractions.Persistence;

public interface IRepository<TEntity>
    where TEntity : Entity
{
    Task<TEntity?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default,
        bool asNoTracking = false,
        params Expression<Func<TEntity, object>>[] includes);

    Task<IReadOnlyList<TEntity>> GetAllAsync(
        CancellationToken cancellationToken = default,
        Expression<Func<TEntity, bool>>? predicate = null,
        params Expression<Func<TEntity, object>>[] includes);

    Task AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default);

    void Update(TEntity entity);

    void Remove(TEntity entity);
}