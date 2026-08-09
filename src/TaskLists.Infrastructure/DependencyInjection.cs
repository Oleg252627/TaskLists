using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskLists.Application.Abstractions.Persistence;
using TaskLists.Application.Abstractions.Repositories;
using TaskLists.Infrastructure.Persistence;
using TaskLists.Infrastructure.Persistence.Interceptors;
using TaskLists.Infrastructure.Persistence.UnitOfWork;
using TaskLists.Infrastructure.Repositories;

namespace TaskLists.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<EntityInterceptor>();

        services.AddDbContext<TaskListsDbContext>((serviceProvider, options) =>
        {
            var interceptor =
                serviceProvider.GetRequiredService<EntityInterceptor>();

            var connectionString =
                configuration.GetConnectionString("Database")
                ?? throw new InvalidOperationException(
                    "Connection string 'Database' was not found.");

            options
                .UseNpgsql(connectionString)
                .AddInterceptors(interceptor);
        });
        
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        services.AddScoped<ITaskListRepository, TaskListRepository>();

        return services;
    }
    
    
}