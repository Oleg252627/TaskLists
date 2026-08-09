namespace TaskLists.Application.Exceptions;

public sealed class ForbiddenException(
    string code,
    string message)
    : ApplicationExceptionBase(code, message);