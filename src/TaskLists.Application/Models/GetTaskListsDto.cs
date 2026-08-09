namespace TaskLists.Application.Models;

public sealed record GetTaskListsDto(
    int Page,
    int PageSize);
