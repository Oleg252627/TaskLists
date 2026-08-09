using Serilog.Context;

namespace TaskLists.Api.Middleware;

public sealed class RequestLogContextMiddleware(
    RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        using (LogContext.PushProperty(
                   "TraceId",
                   context.TraceIdentifier))
        {
            await next(context);
        }
    }
}

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogContext(
        this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestLogContextMiddleware>();
    }
}