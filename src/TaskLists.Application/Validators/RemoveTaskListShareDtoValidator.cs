using FluentValidation;
using TaskLists.Application.Models;

namespace TaskLists.Application.Validators;

public sealed class RemoveTaskListShareDtoValidator : AbstractValidator<RemoveTaskListShareDto>
{
    public RemoveTaskListShareDtoValidator()
    {
        RuleFor(x => x.TaskListId)
            .NotEmpty();

        RuleFor(x => x.TargetUserId)
            .NotEmpty();
    }
}
