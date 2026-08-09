using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TaskLists.Api.Swagger;

public sealed class UserIdHeaderOperationFilter : IOperationFilter
{
    public void Apply(
        OpenApiOperation operation,
        OperationFilterContext context)
    {
        operation.Parameters ??= new List<IOpenApiParameter>();

        if (operation.Parameters.Any(x => x.Name == "X-User-Id"))
        {
            return;
        }

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-User-Id",
            In = ParameterLocation.Header,
            Required = true,
            Description = "Current user id (GUID).",
            Schema = new OpenApiSchema
            {
                Format = "uuid"
            }
        });
    }
}
