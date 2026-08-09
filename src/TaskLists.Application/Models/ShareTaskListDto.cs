namespace TaskLists.Application.Models;

public sealed record ShareTaskListDto(
    Guid TaskListId,
    Guid TargetUserId);