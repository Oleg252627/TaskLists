using Microsoft.EntityFrameworkCore;
using TaskLists.Application.Abstractions.Repositories;
using TaskLists.Domain.Entities;
using TaskLists.Infrastructure.Persistence;

namespace TaskLists.Infrastructure.Repositories;

public sealed class TaskListRepository(
    TaskListsDbContext dbContext)
    : Repository<TaskList>(dbContext),
        ITaskListRepository
{
    public async Task<IReadOnlyList<TaskList>> GetAccessibleByUserAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var skip = (page - 1) * pageSize;

        return await DbContext.TaskLists
            .AsNoTracking()
            .Where(x =>
                x.OwnerId == userId ||
                x.Shares.Any(share => share.UserId == userId))
            .OrderByDescending(x => x.CreatedAtUtc)
            .ThenByDescending(x => x.Id)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<TaskList?> GetForUpdateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var taskList = await DbContext.TaskLists
            .FromSqlInterpolated($"""
                                  SELECT *
                                  FROM "TaskLists"
                                  WHERE "Id" = {id}
                                  FOR UPDATE
                                  """)
            .SingleOrDefaultAsync(cancellationToken);

        if (taskList is null)
            return null;

        await DbContext.Entry(taskList)
            .Collection(x => x.Shares)
            .LoadAsync(cancellationToken);

        return taskList;
    }
}