namespace TaskLists.Application.Exceptions;

public sealed class NotFoundException(
    string code,
    string message)
    : ApplicationExceptionBase(code, message);