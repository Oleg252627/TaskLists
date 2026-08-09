using Microsoft.OpenApi;
using Serilog;
using TaskLists.Api.Endpoints.TaskLists;
using TaskLists.Api.ExceptionHandling;
using TaskLists.Api.Middleware;
using TaskLists.Api.Services;
using TaskLists.Api.Swagger;
using TaskLists.Application;
using TaskLists.Application.Abstractions;
using TaskLists.Infrastructure;
using TaskLists.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ICurrentUser, CurrentUser>();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TaskLists API",
        Version = "v1"
    });
    options.OperationFilter<UserIdHeaderOperationFilter>();
    options.OperationFilter<ValidationProblemExampleOperationFilter>();
});

var app = builder.Build();

app.UseRequestLogContext();

app.UseSerilogRequestLogging();

app.UseExceptionHandler();

var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
await app.Services.ApplyMigrationsAsync(lifetime.ApplicationStopping);

app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint(
        "/swagger/v1/swagger.json",
        "TaskLists API v1");
    options.RoutePrefix = "swagger";
});

app.MapTaskListEndpoints();

app.Run();

public partial class Program { }
