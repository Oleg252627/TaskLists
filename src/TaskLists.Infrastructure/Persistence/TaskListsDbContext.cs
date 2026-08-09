using Microsoft.EntityFrameworkCore;
using TaskLists.Domain.Entities;

namespace TaskLists.Infrastructure.Persistence;

public sealed class TaskListsDbContext(
    DbContextOptions<TaskListsDbContext> options)
    : DbContext(options)
{
    public DbSet<TaskList> TaskLists => Set<TaskList>();

    public DbSet<TaskListShare> TaskListShares => Set<TaskListShare>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(TaskListsDbContext).Assembly);
    }
}