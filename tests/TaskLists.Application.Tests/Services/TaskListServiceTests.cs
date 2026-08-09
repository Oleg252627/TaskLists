using System.Linq.Expressions;
using Moq;
using TaskLists.Application.Abstractions;
using TaskLists.Application.Abstractions.Persistence;
using TaskLists.Application.Abstractions.Repositories;
using TaskLists.Application.Exceptions;
using TaskLists.Application.Models;
using TaskLists.Application.Services;
using TaskLists.Domain.Entities;

namespace TaskLists.Application.Tests.Services;

public sealed class TaskListServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<ITaskListRepository> _taskListRepository = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly TaskListService _service;
    private readonly Guid _userId = Guid.NewGuid();

    public TaskListServiceTests()
    {
        _currentUser.SetupGet(x => x.UserId).Returns(_userId);
        _service = new TaskListService(
            _unitOfWork.Object,
            _taskListRepository.Object,
            _currentUser.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateTaskList_WithTrimmedNameAndOwner()
    {
        var dto = new CreateTaskListDto("  List name  ");

        await _service.CreateAsync(dto);

        _taskListRepository.Verify(
            x => x.AddAsync(
                It.Is<TaskList>(list =>
                    list.Name == "List name" &&
                    list.OwnerId == _userId),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetListAsync_ShouldReturnMappedItems()
    {
        var dto = new GetTaskListsDto(2, 10);
        var lists = new List<TaskList>
        {
            new() { Id = Guid.NewGuid(), Name = "A", OwnerId = _userId },
            new() { Id = Guid.NewGuid(), Name = "B", OwnerId = _userId }
        };
        _taskListRepository
            .Setup(x => x.GetAccessibleByUserAsync(
                _userId,
                dto.Page,
                dto.PageSize,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(lists);

        var result = await _service.GetListAsync(dto);

        Assert.Equal(2, result.Count);
        Assert.Equal(lists[0].Id, result[0].Id);
        Assert.Equal("A", result[0].Name);
        Assert.Equal(lists[1].Id, result[1].Id);
        Assert.Equal("B", result[1].Name);
    }

    [Fact]
    public async Task GetAsync_ShouldThrowNotFound_WhenTaskListDoesNotExist()
    {
        _taskListRepository
            .Setup(x => x.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<TaskList, object>>[]>()))
            .ReturnsAsync((TaskList?)null);

        var action = () => _service.GetAsync(Guid.NewGuid());

        var exception = await Assert.ThrowsAsync<NotFoundException>(action);
        Assert.Equal(ErrorCodes.TaskListNotFound, exception.Code);
    }

    [Fact]
    public async Task GetAsync_ShouldThrowForbidden_WhenUserHasNoAccess()
    {
        var taskList = new TaskList
        {
            Id = Guid.NewGuid(),
            Name = "Private",
            OwnerId = Guid.NewGuid(),
            Shares = []
        };
        _taskListRepository
            .Setup(x => x.GetByIdAsync(
                taskList.Id,
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<TaskList, object>>[]>()))
            .ReturnsAsync(taskList);

        var action = () => _service.GetAsync(taskList.Id);

        var exception = await Assert.ThrowsAsync<ForbiddenException>(action);
        Assert.Equal(ErrorCodes.TaskListAccessDenied, exception.Code);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnDetails_WhenUserHasAccess()
    {
        var sharedUserId = Guid.NewGuid();
        var taskList = new TaskList
        {
            Id = Guid.NewGuid(),
            Name = "Shared",
            OwnerId = Guid.NewGuid(),
            Shares = [new TaskListShare { UserId = _userId }, new TaskListShare { UserId = sharedUserId }]
        };
        _taskListRepository
            .Setup(x => x.GetByIdAsync(
                taskList.Id,
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<TaskList, object>>[]>()))
            .ReturnsAsync(taskList);

        var result = await _service.GetAsync(taskList.Id);

        Assert.Equal(taskList.Id, result.Id);
        Assert.Equal(taskList.Name, result.Name);
        Assert.Equal(2, result.SharedUserIds.Count);
        Assert.Contains(_userId, result.SharedUserIds);
        Assert.Contains(sharedUserId, result.SharedUserIds);
    }

    [Fact]
    public async Task GetSharesAsync_ShouldReturnShareDtos_WhenUserHasAccess()
    {
        var sharedUserId = Guid.NewGuid();
        var taskListId = Guid.NewGuid();
        var taskList = new TaskList
        {
            Id = taskListId,
            Name = "Shared",
            OwnerId = Guid.NewGuid(),
            Shares =
            [
                new TaskListShare { TaskListId = taskListId, UserId = _userId },
                new TaskListShare { TaskListId = taskListId, UserId = sharedUserId }
            ]
        };
        _taskListRepository
            .Setup(x => x.GetByIdAsync(
                taskList.Id,
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<TaskList, object>>[]>()))
            .ReturnsAsync(taskList);

        var result = await _service.GetSharesAsync(taskList.Id);

        Assert.Equal(2, result.Count);
        Assert.Equal(taskListId, result[0].TaskListId);
        Assert.Equal(_userId, result[0].UserId);
        Assert.Equal(taskListId, result[1].TaskListId);
        Assert.Equal(sharedUserId, result[1].UserId);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowForbidden_WhenCurrentUserIsNotOwner()
    {
        var taskList = new TaskList
        {
            Id = Guid.NewGuid(),
            Name = "Task list",
            OwnerId = Guid.NewGuid()
        };
        _taskListRepository
            .Setup(x => x.GetByIdAsync(
                taskList.Id,
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<TaskList, object>>[]>()))
            .ReturnsAsync(taskList);

        var action = () => _service.DeleteAsync(taskList.Id);

        var exception = await Assert.ThrowsAsync<ForbiddenException>(action);
        Assert.Equal(ErrorCodes.TaskListAccessDenied, exception.Code);
        _taskListRepository.Verify(x => x.Remove(It.IsAny<TaskList>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveAndSave_WhenCurrentUserIsOwner()
    {
        var taskList = new TaskList
        {
            Id = Guid.NewGuid(),
            Name = "Task list",
            OwnerId = _userId
        };
        _taskListRepository
            .Setup(x => x.GetByIdAsync(
                taskList.Id,
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<TaskList, object>>[]>()))
            .ReturnsAsync(taskList);

        await _service.DeleteAsync(taskList.Id);

        _taskListRepository.Verify(x => x.Remove(taskList), Times.Once);
        _unitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ShareAsync_ShouldThrowConflict_WhenShareLimitExceeded()
    {
        var taskList = new TaskList
        {
            Id = Guid.NewGuid(),
            Name = "Shared",
            OwnerId = _userId,
            Shares =
            [
                new TaskListShare { UserId = Guid.NewGuid() },
                new TaskListShare { UserId = Guid.NewGuid() },
                new TaskListShare { UserId = Guid.NewGuid() }
            ]
        };
        var dto = new ShareTaskListDto(taskList.Id, Guid.NewGuid());
        var transaction = new Mock<ITransaction>();
        _unitOfWork
            .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction.Object);
        _taskListRepository
            .Setup(x => x.GetForUpdateAsync(taskList.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskList);

        var action = () => _service.ShareAsync(dto);

        var exception = await Assert.ThrowsAsync<ConflictException>(action);
        Assert.Equal(ErrorCodes.TaskListShareLimitExceeded, exception.Code);
        _unitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
        transaction.Verify(
            x => x.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ShareAsync_ShouldAddShareAndCommit_WhenValid()
    {
        var dto = new ShareTaskListDto(Guid.NewGuid(), Guid.NewGuid());
        var taskList = new TaskList
        {
            Id = dto.TaskListId,
            Name = "Shared",
            OwnerId = _userId,
            Shares = []
        };
        var transaction = new Mock<ITransaction>();
        _unitOfWork
            .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction.Object);
        _taskListRepository
            .Setup(x => x.GetForUpdateAsync(dto.TaskListId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskList);

        await _service.ShareAsync(dto);

        Assert.Single(taskList.Shares);
        Assert.Equal(dto.TargetUserId, taskList.Shares.First().UserId);
        _unitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
        transaction.Verify(
            x => x.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RemoveShareAsync_ShouldRemoveShareAndCommit_WhenShareExists()
    {
        var targetUserId = Guid.NewGuid();
        var dto = new RemoveTaskListShareDto(Guid.NewGuid(), targetUserId);
        var taskList = new TaskList
        {
            Id = dto.TaskListId,
            Name = "Shared",
            OwnerId = _userId,
            Shares =
            [
                new TaskListShare { UserId = targetUserId },
                new TaskListShare { UserId = Guid.NewGuid() }
            ]
        };
        var transaction = new Mock<ITransaction>();
        _unitOfWork
            .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction.Object);
        _taskListRepository
            .Setup(x => x.GetForUpdateAsync(dto.TaskListId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskList);

        await _service.RemoveShareAsync(dto);

        Assert.DoesNotContain(taskList.Shares, x => x.UserId == targetUserId);
        _unitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
        transaction.Verify(
            x => x.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
