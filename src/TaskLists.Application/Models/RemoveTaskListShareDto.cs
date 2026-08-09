namespace TaskLists.Application.Models;

public sealed record RemoveTaskListShareDto(
    Guid TaskListId,
    Guid TargetUserId);