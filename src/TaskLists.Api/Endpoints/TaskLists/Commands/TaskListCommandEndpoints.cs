using TaskLists.Application.Models;

namespace TaskLists.Api.Endpoints.TaskLists.Commands;

public static class TaskListCommandEndpoints
{
    public static RouteGroupBuilder MapTaskListCommandEndpoints(
        this RouteGroupBuilder taskLists)
    {
        taskLists.MapPost("/", TaskListCommandHandlers.CreateAsync)
            .WithName("CreateTaskList")
            .WithSummary("Create task list")
            .Accepts<CreateTaskListDto>("application/json")
            .Produces<CreatedTaskListDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        taskLists.MapPut("/{id:guid}", TaskListCommandHandlers.UpdateAsync)
            .WithName("UpdateTaskList")
            .WithSummary("Update task list")
            .Accepts<CreateTaskListDto>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        taskLists.MapDelete("/{id:guid}", TaskListCommandHandlers.DeleteAsync)
            .WithName("DeleteTaskList")
            .WithSummary("Delete task list")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        taskLists.MapPost(
                "/{id:guid}/shares/{targetUserId:guid}",
                TaskListCommandHandlers.ShareAsync)
            .WithName("ShareTaskList")
            .WithSummary("Share task list")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        taskLists.MapDelete(
                "/{id:guid}/shares/{targetUserId:guid}",
                TaskListCommandHandlers.RemoveShareAsync)
            .WithName("RemoveTaskListShare")
            .WithSummary("Remove task list share")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return taskLists;
    }
}
