using TaskLists.Application.Abstractions.Services;
using TaskLists.Application.Models;

namespace TaskLists.Api.Endpoints.TaskLists.Queries;

public static class TaskListQueryHandlers
{
    public static async Task<IResult> GetListAsync(
        int? page,
        int? pageSize,
        ITaskListService taskListService,
        CancellationToken cancellationToken)
    {
        var dto = new GetTaskListsDto(
            page ?? 1,
            pageSize ?? 20);

        var result = await taskListService.GetListAsync(
            dto,
            cancellationToken);

        return Results.Ok(result);
    }

    public static async Task<IResult> GetByIdAsync(
        Guid id,
        ITaskListService taskListService,
        CancellationToken cancellationToken)
    {
        var result = await taskListService.GetAsync(
            id,
            cancellationToken);

        return Results.Ok(result);
    }

    public static async Task<IResult> GetSharesAsync(
        Guid id,
        ITaskListService taskListService,
        CancellationToken cancellationToken)
    {
        var result = await taskListService.GetSharesAsync(
            id,
            cancellationToken);

        return Results.Ok(result);
    }
}
