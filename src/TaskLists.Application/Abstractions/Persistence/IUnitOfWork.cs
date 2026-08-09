using TaskLists.Domain.Common;

namespace TaskLists.Application.Abstractions.Persistence;

public interface IUnitOfWork
{
    IRepository<TEntity> Repository<TEntity>()
        where TEntity : Entity;

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);

    Task<ITransaction> BeginTransactionAsync(
        CancellationToken cancellationToken = default);
}