using TaskLists.Application.Abstractions.Persistence;
using TaskLists.Domain.Entities;

namespace TaskLists.Application.Abstractions.Repositories;

public interface ITaskListRepository : IRepository<TaskList>
{
    Task<TaskList?> GetForUpdateAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<TaskList>> GetAccessibleByUserAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}