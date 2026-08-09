using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TaskLists.Application.Abstractions.Validation;

namespace TaskLists.Application.Validation;

public sealed class ValidationService(
    IServiceProvider serviceProvider)
    : IValidationService
{
    public async Task ValidateAndThrowAsync<T>(
        T model,
        CancellationToken cancellationToken = default)
    {
        var validator = serviceProvider.GetRequiredService<IValidator<T>>();

        var result = await validator.ValidateAsync(
            model,
            cancellationToken);

        if (result.IsValid)
        {
            return;
        }

        var errors = result.Errors
            .GroupBy(x => x.PropertyName)
            .ToDictionary(
                x => x.Key,
                x => x
                    .Select(error => error.ErrorMessage)
                    .Distinct()
                    .ToArray());

        throw new TaskLists.Application.Exceptions.ValidationException(errors);
    }
}
