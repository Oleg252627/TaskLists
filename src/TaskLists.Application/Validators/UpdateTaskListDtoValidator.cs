using FluentValidation;
using TaskLists.Application.Models;

namespace TaskLists.Application.Validators;

public sealed class UpdateTaskListDtoValidator : AbstractValidator<UpdateTaskListDto>
{
    public UpdateTaskListDtoValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Name)
            .Custom((name, context) =>
            {
                var length = name?.Trim().Length ?? 0;

                if (length is < 1 or > 255)
                {
                    context.AddFailure("Name length must be between 1 and 255 characters.");
                }
            });
    }
}
