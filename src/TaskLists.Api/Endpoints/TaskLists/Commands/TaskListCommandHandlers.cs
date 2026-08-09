using Microsoft.AspNetCore.Mvc;
using TaskLists.Application.Abstractions.Services;
using TaskLists.Application.Models;

namespace TaskLists.Api.Endpoints.TaskLists.Commands;

public static class TaskListCommandHandlers
{
    public static async Task<IResult> CreateAsync(
        [FromBody] CreateTaskListDto dto,
        ITaskListService taskListService,
        CancellationToken cancellationToken)
    {
        var id = await taskListService.CreateAsync(
            dto,
            cancellationToken);

        return Results.Created(
            $"/task-lists/{id}",
            new CreatedTaskListDto(id));
    }

    public static async Task<IResult> UpdateAsync(
        Guid id,
        [FromBody] CreateTaskListDto dto,
        ITaskListService taskListService,
        CancellationToken cancellationToken)
    {
        await taskListService.UpdateAsync(
            new UpdateTaskListDto(id, dto.Name),
            cancellationToken);

        return Results.NoContent();
    }

    public static async Task<IResult> DeleteAsync(
        Guid id,
        ITaskListService taskListService,
        CancellationToken cancellationToken)
    {
        await taskListService.DeleteAsync(
            id,
            cancellationToken);

        return Results.NoContent();
    }

    public static async Task<IResult> ShareAsync(
        Guid id,
        Guid targetUserId,
        ITaskListService taskListService,
        CancellationToken cancellationToken)
    {
        await taskListService.ShareAsync(
            new ShareTaskListDto(id, targetUserId),
            cancellationToken);

        return Results.NoContent();
    }

    public static async Task<IResult> RemoveShareAsync(
        Guid id,
        Guid targetUserId,
        ITaskListService taskListService,
        CancellationToken cancellationToken)
    {
        await taskListService.RemoveShareAsync(
            new RemoveTaskListShareDto(id, targetUserId),
            cancellationToken);

        return Results.NoContent();
    }
}
