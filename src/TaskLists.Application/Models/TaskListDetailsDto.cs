namespace TaskLists.Application.Models;

public sealed record TaskListDetailsDto(
    Guid Id,
    string Name,
    Guid OwnerId,
    DateTime CreatedAtUtc,
    IReadOnlyCollection<Guid> SharedUserIds);