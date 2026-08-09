using TaskLists.Application.Abstractions.Services;
using TaskLists.Application.Abstractions.Validation;
using TaskLists.Application.Models;

namespace TaskLists.Application.Services;

public sealed class ValidatedTaskListService(
    TaskListService inner,
    IValidationService validationService)
    : ITaskListService
{
    public async Task<Guid> CreateAsync(
        CreateTaskListDto dto,
        CancellationToken cancellationToken = default)
    {
        await validationService.ValidateAndThrowAsync(
            dto,
            cancellationToken);

        return await inner.CreateAsync(
            dto,
            cancellationToken);
    }

    public Task<TaskListDetailsDto> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return inner.GetAsync(
            id,
            cancellationToken);
    }

    public async Task<IReadOnlyList<TaskListListItemDto>> GetListAsync(
        GetTaskListsDto dto,
        CancellationToken cancellationToken = default)
    {
        await validationService.ValidateAndThrowAsync(
            dto,
            cancellationToken);

        return await inner.GetListAsync(
            dto,
            cancellationToken);
    }

    public Task<IReadOnlyList<TaskListShareDto>> GetSharesAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return inner.GetSharesAsync(
            id,
            cancellationToken);
    }

    public async Task UpdateAsync(
        UpdateTaskListDto dto,
        CancellationToken cancellationToken = default)
    {
        await validationService.ValidateAndThrowAsync(
            dto,
            cancellationToken);

        await inner.UpdateAsync(
            dto,
            cancellationToken);
    }

    public Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return inner.DeleteAsync(
            id,
            cancellationToken);
    }

    public async Task ShareAsync(
        ShareTaskListDto dto,
        CancellationToken cancellationToken = default)
    {
        await validationService.ValidateAndThrowAsync(
            dto,
            cancellationToken);

        await inner.ShareAsync(
            dto,
            cancellationToken);
    }

    public async Task RemoveShareAsync(
        RemoveTaskListShareDto dto,
        CancellationToken cancellationToken = default)
    {
        await validationService.ValidateAndThrowAsync(
            dto,
            cancellationToken);

        await inner.RemoveShareAsync(
            dto,
            cancellationToken);
    }
}
