using TaskLists.Api.Endpoints.TaskLists.Commands;
using TaskLists.Api.Endpoints.TaskLists.Queries;

namespace TaskLists.Api.Endpoints.TaskLists;

public static class TaskListEndpoints
{
    public static IEndpointRouteBuilder MapTaskListEndpoints(
        this IEndpointRouteBuilder app)
    {
        var taskLists = app.MapGroup("/task-lists");

        taskLists.MapTaskListCommandEndpoints();
        taskLists.MapTaskListQueryEndpoints();

        return app;
    }
}
