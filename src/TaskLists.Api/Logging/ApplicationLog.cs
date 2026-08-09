namespace TaskLists.Api.Logging;

public static partial class ApplicationLog
{
    [LoggerMessage(
        EventId = 1000,
        Level = LogLevel.Warning,
        Message = "Validation failed. ErrorCode: {ErrorCode}")]
    public static partial void ValidationFailed(
        ILogger logger,
        string errorCode);

    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Warning,
        Message = "Resource not found. ErrorCode: {ErrorCode}")]
    public static partial void NotFound(
        ILogger logger,
        string errorCode);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Warning,
        Message = "Access denied. ErrorCode: {ErrorCode}")]
    public static partial void Forbidden(
        ILogger logger,
        string errorCode);

    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Warning,
        Message = "Conflict occurred. ErrorCode: {ErrorCode}")]
    public static partial void Conflict(
        ILogger logger,
        string errorCode);

    [LoggerMessage(
        EventId = 5000,
        Level = LogLevel.Error,
        Message = "Unhandled exception occurred")]
    public static partial void UnhandledException(
        ILogger logger,
        Exception exception);
}