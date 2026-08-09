using TaskLists.Domain.Common;

namespace TaskLists.Domain.Entities;

public sealed class TaskList : Entity
{
    public required string Name { get; set; }

    public Guid OwnerId { get; set; }

    public ICollection<TaskListShare> Shares { get; set; } = [];
}