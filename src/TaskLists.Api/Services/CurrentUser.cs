using TaskLists.Application.Abstractions;
using TaskLists.Application.Exceptions;

namespace TaskLists.Api.Services;

public sealed class CurrentUser(
    IHttpContextAccessor httpContextAccessor)
    : ICurrentUser
{
    public Guid UserId
    {
        get
        {
            var value = httpContextAccessor
                .HttpContext?
                .Request
                .Headers["X-User-Id"]
                .FirstOrDefault();

            if (!Guid.TryParse(value, out var userId))
            {
                throw new ValidationException(
                    new Dictionary<string, string[]>
                    {
                        ["X-User-Id"] =
                        [
                            "X-User-Id header is missing or invalid GUID."
                        ]
                    });
            }

            return userId;
        }
    }
}