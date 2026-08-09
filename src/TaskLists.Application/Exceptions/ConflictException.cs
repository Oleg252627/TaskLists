namespace TaskLists.Application.Exceptions;

public sealed class ConflictException(
    string code,
    string message)
    : ApplicationExceptionBase(code, message);