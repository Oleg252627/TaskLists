namespace TaskLists.Domain.Common;

public abstract class Entity : ICreatedAt
{
    public Guid Id { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}