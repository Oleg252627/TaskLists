using Microsoft.Extensions.DependencyInjection;
using TaskLists.Application.Abstractions.Persistence;
using TaskLists.Domain.Common;
using TaskLists.Infrastructure.Persistence.Transactions;

namespace TaskLists.Infrastructure.Persistence.UnitOfWork;

public sealed class UnitOfWork(
    TaskListsDbContext dbContext,
    IServiceProvider serviceProvider)
    : IUnitOfWork
{
    public IRepository<TEntity> Repository<TEntity>()
        where TEntity : Entity
    {
        return serviceProvider
            .GetRequiredService<IRepository<TEntity>>();
    }

    public Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<ITransaction> BeginTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        var transaction =
            await dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        return new EfTransaction(transaction);
    }
}