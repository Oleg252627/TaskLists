using Microsoft.EntityFrameworkCore.Storage;
using TaskLists.Application.Abstractions.Persistence;

namespace TaskLists.Infrastructure.Persistence.Transactions;

public sealed class EfTransaction(
    IDbContextTransaction transaction)
    : ITransaction
{
    public Task CommitAsync(
        CancellationToken cancellationToken = default)
    {
        return transaction.CommitAsync(cancellationToken);
    }

    public Task RollbackAsync(
        CancellationToken cancellationToken = default)
    {
        return transaction.RollbackAsync(cancellationToken);
    }

    public ValueTask DisposeAsync()
    {
        return transaction.DisposeAsync();
    }
}