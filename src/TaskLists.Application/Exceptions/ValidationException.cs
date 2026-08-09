namespace TaskLists.Application.Exceptions;

public sealed class ValidationException
    : ApplicationExceptionBase
{
    public ValidationException(
        IReadOnlyDictionary<string, string[]> errors)
        : base(
            ErrorCodes.ValidationFailed,
            "One or more validation errors occurred.")
    {
        Errors = errors;
    }

    public IReadOnlyDictionary<string, string[]> Errors { get; }
}