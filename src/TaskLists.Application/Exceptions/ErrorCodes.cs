namespace TaskLists.Application.Exceptions;

public static class ErrorCodes
{
    public const string ValidationFailed = "ValidationFailed";

    public const string TaskListNotFound = "TaskListNotFound";

    public const string TaskListAccessDenied = "TaskListAccessDenied";

    public const string TaskListShareLimitExceeded = "TaskListShareLimitExceeded";

    public const string TaskListAlreadyShared = "TaskListAlreadyShared";
}