using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskLists.Domain.Entities;

namespace TaskLists.Infrastructure.Persistence.Configurations;

public sealed class TaskListConfiguration
    : IEntityTypeConfiguration<TaskList>
{
    public void Configure(EntityTypeBuilder<TaskList> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Name)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.OwnerId)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.HasMany(x => x.Shares)
            .WithOne(x => x.TaskList)
            .HasForeignKey(x => x.TaskListId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.OwnerId);

        builder.HasIndex(x => x.CreatedAtUtc);

        builder.HasIndex(x => new
        {
            x.OwnerId,
            x.CreatedAtUtc,
            x.Id
        });
    }
}