using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace TaskLists.Infrastructure.Persistence;

public static class DatabaseExtensions
{
    public static async Task ApplyMigrationsAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();

        var dbContext =
            scope.ServiceProvider.GetRequiredService<TaskListsDbContext>();

        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}