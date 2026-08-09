using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskLists.Application.Models;
using TaskLists.Infrastructure.Persistence;

namespace TaskLists.Api.Tests.Integration;

public sealed class TaskListsEndpointsTests : IClassFixture<PostgresFixture>
{
    private readonly PostgresFixture _fixture;

    public TaskListsEndpointsTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task PostTaskList_ShouldPersistOwnerAndName()
    {
        using var factory = new TaskListsApiFactory(_fixture.ConnectionString);
        await ResetDatabaseAsync(factory);
        using var client = factory.CreateClient();

        var userId = Guid.NewGuid();
        var response = await SendAsync(
            client,
            HttpMethod.Post,
            "/task-lists",
            userId,
            new CreateTaskListDto("  Team backlog  "));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<CreatedTaskListDto>();
        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created.Id);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TaskListsDbContext>();

        var taskList = await db.TaskLists.SingleAsync();

        Assert.Equal("Team backlog", taskList.Name);
        Assert.Equal(userId, taskList.OwnerId);
    }

    [Fact]
    public async Task GetTaskLists_ShouldReturnOnlyAccessibleLists()
    {
        using var factory = new TaskListsApiFactory(_fixture.ConnectionString);
        await ResetDatabaseAsync(factory);
        using var client = factory.CreateClient();

        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        await SendAsync(
            client,
            HttpMethod.Post,
            "/task-lists",
            userId,
            new CreateTaskListDto("Owned list"));

        await SendAsync(
            client,
            HttpMethod.Post,
            "/task-lists",
            otherUserId,
            new CreateTaskListDto("Shared list"));

        var sharedListId = await GetTaskListIdByNameAsync(factory, "Shared list");

        var shareResponse = await SendAsync(
            client,
            HttpMethod.Post,
            $"/task-lists/{sharedListId}/shares/{userId}",
            otherUserId);

        Assert.Equal(HttpStatusCode.NoContent, shareResponse.StatusCode);

        var response = await SendAsync(
            client,
            HttpMethod.Get,
            "/task-lists?page=1&pageSize=10",
            userId);

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<TaskListListItemDto[]>();

        Assert.NotNull(payload);
        Assert.Equal(2, payload.Length);
        Assert.Contains(payload, x => x.Name == "Owned list");
        Assert.Contains(payload, x => x.Name == "Shared list");
    }

    [Fact]
    public async Task GetTaskListShares_ShouldReturnShares_WhenUserHasAccess()
    {
        using var factory = new TaskListsApiFactory(_fixture.ConnectionString);
        await ResetDatabaseAsync(factory);
        using var client = factory.CreateClient();

        var ownerId = Guid.NewGuid();
        var sharedUserId = Guid.NewGuid();

        await SendAsync(
            client,
            HttpMethod.Post,
            "/task-lists",
            ownerId,
            new CreateTaskListDto("Shared"));

        var sharedTaskListId = await GetTaskListIdByNameAsync(factory, "Shared");

        await SendAsync(
            client,
            HttpMethod.Post,
            $"/task-lists/{sharedTaskListId}/shares/{sharedUserId}",
            ownerId);

        var response = await SendAsync(
            client,
            HttpMethod.Get,
            $"/task-lists/{sharedTaskListId}/shares",
            sharedUserId);

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<TaskListShareDto[]>();

        Assert.NotNull(payload);
        Assert.Single(payload);
        Assert.Equal(sharedTaskListId, payload[0].TaskListId);
        Assert.Equal(sharedUserId, payload[0].UserId);
    }

    [Fact]
    public async Task ShareTaskList_ShouldReturnConflict_WhenShareLimitExceeded()
    {
        using var factory = new TaskListsApiFactory(_fixture.ConnectionString);
        await ResetDatabaseAsync(factory);
        using var client = factory.CreateClient();

        var ownerId = Guid.NewGuid();
        var targetUserIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        await SendAsync(
            client,
            HttpMethod.Post,
            "/task-lists",
            ownerId,
            new CreateTaskListDto("Shared"));

        var taskListId = await GetTaskListIdByNameAsync(factory, "Shared");

        foreach (var targetUserId in targetUserIds[..3])
        {
            var shareResponse = await SendAsync(
                client,
                HttpMethod.Post,
                $"/task-lists/{taskListId}/shares/{targetUserId}",
                ownerId);

            Assert.Equal(HttpStatusCode.NoContent, shareResponse.StatusCode);
        }

        var conflictResponse = await SendAsync(
            client,
            HttpMethod.Post,
            $"/task-lists/{taskListId}/shares/{targetUserIds[3]}",
            ownerId);

        Assert.Equal(HttpStatusCode.Conflict, conflictResponse.StatusCode);
    }

    [Fact]
    public async Task UpdateTaskList_ShouldAllowSharedUser()
    {
        using var factory = new TaskListsApiFactory(_fixture.ConnectionString);
        await ResetDatabaseAsync(factory);
        using var client = factory.CreateClient();

        var ownerId = Guid.NewGuid();
        var sharedUserId = Guid.NewGuid();

        await SendAsync(
            client,
            HttpMethod.Post,
            "/task-lists",
            ownerId,
            new CreateTaskListDto("Original"));

        var taskListId = await GetTaskListIdByNameAsync(factory, "Original");

        await SendAsync(
            client,
            HttpMethod.Post,
            $"/task-lists/{taskListId}/shares/{sharedUserId}",
            ownerId);

        var updateResponse = await SendAsync(
            client,
            HttpMethod.Put,
            $"/task-lists/{taskListId}",
            sharedUserId,
            new CreateTaskListDto("Updated by shared user"));

        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TaskListsDbContext>();
        var updatedTaskList = await db.TaskLists.SingleAsync(x => x.Id == taskListId);

        Assert.Equal("Updated by shared user", updatedTaskList.Name);
    }

    [Fact]
    public async Task RemoveShare_ShouldAllowSharedUserToRemoveOwnLink()
    {
        using var factory = new TaskListsApiFactory(_fixture.ConnectionString);
        await ResetDatabaseAsync(factory);
        using var client = factory.CreateClient();

        var ownerId = Guid.NewGuid();
        var sharedUserId = Guid.NewGuid();

        await SendAsync(
            client,
            HttpMethod.Post,
            "/task-lists",
            ownerId,
            new CreateTaskListDto("Shared"));

        var taskListId = await GetTaskListIdByNameAsync(factory, "Shared");

        await SendAsync(
            client,
            HttpMethod.Post,
            $"/task-lists/{taskListId}/shares/{sharedUserId}",
            ownerId);

        var removeResponse = await SendAsync(
            client,
            HttpMethod.Delete,
            $"/task-lists/{taskListId}/shares/{sharedUserId}",
            sharedUserId);

        Assert.Equal(HttpStatusCode.NoContent, removeResponse.StatusCode);

        var sharesResponse = await SendAsync(
            client,
            HttpMethod.Get,
            $"/task-lists/{taskListId}/shares",
            ownerId);

        sharesResponse.EnsureSuccessStatusCode();

        var payload = await sharesResponse.Content.ReadFromJsonAsync<TaskListShareDto[]>();

        Assert.NotNull(payload);
        Assert.Empty(payload);
    }

    [Fact]
    public async Task DeleteTaskList_ShouldReturnForbidden_WhenUserIsNotOwner()
    {
        using var factory = new TaskListsApiFactory(_fixture.ConnectionString);
        await ResetDatabaseAsync(factory);
        using var client = factory.CreateClient();

        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        await SendAsync(
            client,
            HttpMethod.Post,
            "/task-lists",
            ownerId,
            new CreateTaskListDto("Private"));

        var taskListId = await GetTaskListIdByNameAsync(factory, "Private");

        var response = await SendAsync(
            client,
            HttpMethod.Delete,
            $"/task-lists/{taskListId}",
            otherUserId);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetTaskLists_ShouldReturnValidationProblem_WhenUserIdHeaderMissing()
    {
        using var factory = new TaskListsApiFactory(_fixture.ConnectionString);
        await ResetDatabaseAsync(factory);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/task-lists");
        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("ValidationFailed", body);
        Assert.True(json.RootElement.TryGetProperty("errors", out var errors));
        Assert.True(errors.TryGetProperty("X-User-Id", out _));
    }

    private static async Task<HttpResponseMessage> SendAsync<T>(
        HttpClient client,
        HttpMethod method,
        string url,
        Guid userId,
        T body)
    {
        using var request = new HttpRequestMessage(method, url)
        {
            Content = JsonContent.Create(body)
        };
        request.Headers.Add("X-User-Id", userId.ToString());

        return await client.SendAsync(request);
    }

    private static async Task<HttpResponseMessage> SendAsync(
        HttpClient client,
        HttpMethod method,
        string url,
        Guid userId)
    {
        using var request = new HttpRequestMessage(method, url);
        request.Headers.Add("X-User-Id", userId.ToString());

        return await client.SendAsync(request);
    }

    private static async Task ResetDatabaseAsync(TaskListsApiFactory factory)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TaskListsDbContext>();

        await db.Database.ExecuteSqlRawAsync(
            """
            TRUNCATE TABLE "TaskListShares", "TaskLists" RESTART IDENTITY CASCADE
            """);
    }

    private static async Task<Guid> GetTaskListIdByNameAsync(
        TaskListsApiFactory factory,
        string name)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TaskListsDbContext>();

        return await db.TaskLists
            .Where(x => x.Name == name)
            .Select(x => x.Id)
            .SingleAsync();
    }
}
