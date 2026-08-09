using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskLists.Domain.Entities;

namespace TaskLists.Infrastructure.Persistence.Configurations;

public class TaskListShareConfiguration : IEntityTypeConfiguration<TaskListShare>
{
    public void Configure(EntityTypeBuilder<TaskListShare> builder)
    {
        builder.HasKey(x => new
        {
            x.TaskListId,
            x.UserId
        });

        builder.Property(x => x.TaskListId)
            .IsRequired();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(x => x.UserId);
    }
}