using TaskLists.Application.Models;

namespace TaskLists.Application.Abstractions.Services;

public interface ITaskListService
{
    Task<Guid> CreateAsync(
        CreateTaskListDto dto,
        CancellationToken cancellationToken = default);

    Task<TaskListDetailsDto> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TaskListListItemDto>> GetListAsync(
        GetTaskListsDto dto,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TaskListShareDto>> GetSharesAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        UpdateTaskListDto dto,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task ShareAsync(
        ShareTaskListDto dto,
        CancellationToken cancellationToken = default);

    Task RemoveShareAsync(
        RemoveTaskListShareDto dto,
        CancellationToken cancellationToken = default);
}