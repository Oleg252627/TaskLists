using Microsoft.OpenApi;
using System.Text.Json.Nodes;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TaskLists.Api.Swagger;

public sealed class ValidationProblemExampleOperationFilter : IOperationFilter
{
    public void Apply(
        OpenApiOperation operation,
        OperationFilterContext context)
    {
        if (operation.Responses is null)
        {
            return;
        }

        SetRequestExample(operation);
        SetExample(
            operation,
            "400",
            CreateValidationExample());
        SetExample(
            operation,
            "403",
            CreateProblemExample(
                403,
                "Forbidden",
                "Access denied.",
                "Forbidden"));
        SetExample(
            operation,
            "404",
            CreateProblemExample(
                404,
                "NotFound",
                "The requested resource was not found.",
                "NotFound"));
        SetExample(
            operation,
            "409",
            CreateProblemExample(
                409,
                "Conflict",
                "The requested operation conflicts with the current state.",
                "Conflict"));
    }

    private static void SetRequestExample(OpenApiOperation operation)
    {
        var requestBody = operation.RequestBody;
        if (requestBody?.Content is null)
        {
            return;
        }

        if (!requestBody.Content.TryGetValue("application/json", out var content))
        {
            return;
        }

        content.Example = new JsonObject
        {
            ["name"] = "string"
        };
    }

    private static void SetExample(
        OpenApiOperation operation,
        string statusCode,
        JsonNode example)
    {
        var responses = operation.Responses;
        if (responses is null)
        {
            return;
        }

        if (!responses.TryGetValue(statusCode, out var response))
        {
            return;
        }

        if (response.Content is null)
        {
            return;
        }

        if (!response.Content.TryGetValue("application/problem+json", out var content))
        {
            return;
        }

        content.Example = example;
    }

    private static JsonObject CreateValidationExample()
    {
        return new JsonObject
        {
            ["type"] = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            ["title"] = "ValidationFailed",
            ["status"] = 400,
            ["instance"] = "/task-lists",
            ["code"] = "ValidationFailed",
            ["traceId"] = "00-00000000000000000000000000000000-0000000000000000-00",
            ["errors"] = new JsonObject
            {
                ["name"] = new JsonArray
                {
                    "Name must be between 1 and 255 characters."
                }
            }
        };
    }

    private static JsonObject CreateProblemExample(
        int status,
        string title,
        string detail,
        string code)
    {
        return new JsonObject
        {
            ["type"] = $"https://httpstatuses.com/{status}",
            ["title"] = title,
            ["status"] = status,
            ["detail"] = detail,
            ["instance"] = "/task-lists/00000000-0000-0000-0000-000000000000",
            ["code"] = code,
            ["traceId"] = "00-00000000000000000000000000000000-0000000000000000-00"
        };
    }
}
