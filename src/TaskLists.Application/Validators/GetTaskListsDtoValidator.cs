using FluentValidation;
using TaskLists.Application.Models;

namespace TaskLists.Application.Validators;

public sealed class GetTaskListsDtoValidator : AbstractValidator<GetTaskListsDto>
{
    public GetTaskListsDtoValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);
    }
}
