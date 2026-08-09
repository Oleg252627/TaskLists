using Microsoft.AspNetCore.Http;
using TaskLists.Api.Services;
using TaskLists.Application.Exceptions;

namespace TaskLists.Api.Tests.Services;

public sealed class CurrentUserTests
{
    [Fact]
    public void UserId_ShouldReturnGuid_WhenHeaderIsValid()
    {
        var expectedUserId = Guid.NewGuid();
        var context = new DefaultHttpContext();
        context.Request.Headers["X-User-Id"] = expectedUserId.ToString();
        var accessor = new HttpContextAccessor
        {
            HttpContext = context
        };
        var currentUser = new CurrentUser(accessor);

        var actualUserId = currentUser.UserId;

        Assert.Equal(expectedUserId, actualUserId);
    }

    [Fact]
    public void UserId_ShouldThrowValidationException_WhenHeaderIsMissing()
    {
        var context = new DefaultHttpContext();
        var accessor = new HttpContextAccessor
        {
            HttpContext = context
        };
        var currentUser = new CurrentUser(accessor);

        var exception = Assert.Throws<ValidationException>(() => _ = currentUser.UserId);

        Assert.True(exception.Errors.ContainsKey("X-User-Id"));
    }

    [Fact]
    public void UserId_ShouldThrowValidationException_WhenHeaderIsInvalid()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["X-User-Id"] = "invalid-guid";
        var accessor = new HttpContextAccessor
        {
            HttpContext = context
        };
        var currentUser = new CurrentUser(accessor);

        var exception = Assert.Throws<ValidationException>(() => _ = currentUser.UserId);

        Assert.True(exception.Errors.ContainsKey("X-User-Id"));
    }
}
