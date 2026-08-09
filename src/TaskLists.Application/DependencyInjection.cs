using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskLists.Application.Abstractions.Services;
using TaskLists.Application.Abstractions.Validation;
using TaskLists.Application.Services;
using TaskLists.Application.Validation;

namespace TaskLists.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddValidatorsFromAssemblyContaining<TaskListService>();
        services.AddScoped<IValidationService, ValidationService>();
        services.AddScoped<TaskListService>();
        services.AddScoped<ITaskListService, ValidatedTaskListService>();
        
        return services;
    }
}