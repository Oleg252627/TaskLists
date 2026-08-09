using TaskLists.Application.Abstractions;
using TaskLists.Application.Abstractions.Persistence;
using TaskLists.Application.Abstractions.Repositories;
using TaskLists.Application.Abstractions.Services;
using TaskLists.Application.Exceptions;
using TaskLists.Application.Models;
using TaskLists.Domain.Entities;

namespace TaskLists.Application.Services;

public sealed class TaskListService(
    IUnitOfWork unitOfWork,
    ITaskListRepository taskListRepository,
    ICurrentUser currentUser)
    : ITaskListService
{
    public async Task<Guid> CreateAsync(
        CreateTaskListDto dto,
        CancellationToken cancellationToken = default)
    {
        var taskList = new TaskList
        {
            Name = dto.Name.Trim(),
            OwnerId = currentUser.UserId
        };

        await taskListRepository.AddAsync(
            taskList,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return taskList.Id;
    }

    public async Task<TaskListDetailsDto> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var taskList = await taskListRepository.GetByIdAsync(
            id,
            cancellationToken,
            x => x.Shares);

        if (taskList is null)
        {
            throw new NotFoundException(
                ErrorCodes.TaskListNotFound,
                $"Task list '{id}' was not found.");
        }

        EnsureHasAccess(taskList);

        return new TaskListDetailsDto(
            taskList.Id,
            taskList.Name,
            taskList.OwnerId,
            taskList.CreatedAtUtc,
            taskList.Shares
                .Select(x => x.UserId)
                .ToArray());
    }

    public async Task<IReadOnlyList<TaskListListItemDto>> GetListAsync(
        GetTaskListsDto dto,
        CancellationToken cancellationToken = default)
    {
        var taskLists = await taskListRepository.GetAccessibleByUserAsync(
            currentUser.UserId,
            dto.Page,
            dto.PageSize,
            cancellationToken);

        return taskLists
            .Select(x => new TaskListListItemDto(x.Id, x.Name))
            .ToArray();
    }

    public async Task<IReadOnlyList<TaskListShareDto>> GetSharesAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var taskList = await taskListRepository.GetByIdAsync(
            id,
            cancellationToken,
            x => x.Shares);

        if (taskList is null)
        {
            throw new NotFoundException(
                ErrorCodes.TaskListNotFound,
                $"Task list '{id}' was not found.");
        }

        EnsureHasAccess(taskList);

        return taskList.Shares
            .Select(x => new TaskListShareDto(
                x.TaskListId,
                x.UserId))
            .ToArray();
    }

    public async Task UpdateAsync(
        UpdateTaskListDto dto,
        CancellationToken cancellationToken = default)
    {
        var taskList = await taskListRepository.GetByIdAsync(
            dto.Id,
            cancellationToken,
            x => x.Shares);

        if (taskList is null)
        {
            throw new NotFoundException(
                ErrorCodes.TaskListNotFound,
                $"Task list '{dto.Id}' was not found.");
        }

        EnsureHasAccess(taskList);

        taskList.Name = dto.Name.Trim();

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {

        var taskList = await taskListRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (taskList is null)
        {
            throw new NotFoundException(
                ErrorCodes.TaskListNotFound,
                $"Task list '{id}' was not found.");
        }

        if (taskList.OwnerId != currentUser.UserId)
        {
            throw new ForbiddenException(
                ErrorCodes.TaskListAccessDenied,
                "Only the owner can delete the task list.");
        }

        taskListRepository.Remove(taskList);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ShareAsync(
        ShareTaskListDto dto,
        CancellationToken cancellationToken = default)
    {
        await using var transaction =
            await unitOfWork.BeginTransactionAsync(cancellationToken);

        var taskList = await taskListRepository.GetForUpdateAsync(
            dto.TaskListId,
            cancellationToken);

        if (taskList is null)
        {
            throw new NotFoundException(
                ErrorCodes.TaskListNotFound,
                $"Task list '{dto.TaskListId}' was not found.");
        }

        EnsureHasAccess(taskList);

        if (taskList.OwnerId == dto.TargetUserId)
        {
            return;
        }

        if (taskList.Shares.Any(x => x.UserId == dto.TargetUserId))
        {
            return;
        }

        if (taskList.Shares.Count >= 3)
        {
            throw new ConflictException(
                ErrorCodes.TaskListShareLimitExceeded,
                "A task list cannot be shared with more than three users.");
        }

        taskList.Shares.Add(new TaskListShare
        {
            TaskListId = taskList.Id,
            UserId = dto.TargetUserId
        });

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task RemoveShareAsync(
        RemoveTaskListShareDto dto,
        CancellationToken cancellationToken = default)
    {
        await using var transaction =
            await unitOfWork.BeginTransactionAsync(cancellationToken);

        var taskList = await taskListRepository.GetForUpdateAsync(
            dto.TaskListId,
            cancellationToken);

        if (taskList is null)
        {
            throw new NotFoundException(
                ErrorCodes.TaskListNotFound,
                $"Task list '{dto.TaskListId}' was not found.");
        }

        EnsureHasAccess(taskList);

        var share = taskList.Shares
            .FirstOrDefault(x => x.UserId == dto.TargetUserId);

        if (share is null)
        {
            return;
        }

        taskList.Shares.Remove(share);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

    private void EnsureHasAccess(TaskList taskList)
    {
        var hasAccess =
            taskList.OwnerId == currentUser.UserId ||
            taskList.Shares.Any(
                x => x.UserId == currentUser.UserId);

        if (!hasAccess)
        {
            throw new ForbiddenException(
                ErrorCodes.TaskListAccessDenied,
                "You do not have access to this task list.");
        }
    }

}