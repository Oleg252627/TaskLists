using TaskLists.Domain.Common;

namespace TaskLists.Domain.Entities;

public sealed class TaskListShare : ICreatedAt
{
    public Guid TaskListId { get; set; }

    public Guid UserId { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public TaskList TaskList { get; set; } = null!;
}