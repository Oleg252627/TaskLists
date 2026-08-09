namespace TaskLists.Application.Models;

public sealed record TaskListShareDto(
    Guid TaskListId,
    Guid UserId);
