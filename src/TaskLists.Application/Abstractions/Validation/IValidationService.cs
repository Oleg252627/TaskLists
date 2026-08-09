namespace TaskLists.Application.Abstractions.Validation;

public interface IValidationService
{
    Task ValidateAndThrowAsync<T>(
        T model,
        CancellationToken cancellationToken = default);
}
