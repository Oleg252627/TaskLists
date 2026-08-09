using TaskLists.Application.Models;

namespace TaskLists.Api.Endpoints.TaskLists.Queries;

public static class TaskListQueryEndpoints
{
    public static RouteGroupBuilder MapTaskListQueryEndpoints(
        this RouteGroupBuilder taskLists)
    {
        taskLists.MapGet("/", TaskListQueryHandlers.GetListAsync)
            .WithName("GetTaskLists")
            .WithSummary("Get task lists")
            .Produces<IReadOnlyList<TaskListListItemDto>>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        taskLists.MapGet("/{id:guid}", TaskListQueryHandlers.GetByIdAsync)
            .WithName("GetTaskList")
            .WithSummary("Get task list by id")
            .Produces<TaskListDetailsDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        taskLists.MapGet("/{id:guid}/shares", TaskListQueryHandlers.GetSharesAsync)
            .WithName("GetTaskListShares")
            .WithSummary("Get task list shares")
            .Produces<IReadOnlyList<TaskListShareDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return taskLists;
    }
}
