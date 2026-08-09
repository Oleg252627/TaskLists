using FluentValidation;
using TaskLists.Application.Models;

namespace TaskLists.Application.Validators;

public sealed class ShareTaskListDtoValidator : AbstractValidator<ShareTaskListDto>
{
    public ShareTaskListDtoValidator()
    {
        RuleFor(x => x.TaskListId)
            .NotEmpty();

        RuleFor(x => x.TargetUserId)
            .NotEmpty();
    }
}
