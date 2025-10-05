using FluentValidation;
using StretchRoom.Infrastructure.TestApplication.BoundedContext.Requests;

namespace StretchRoom.Infrastructure.TestApplication.Validators;

public class ChangeNameRequestValidator : AbstractValidator<ChangeNameRequest>
{
    public ChangeNameRequestValidator()
    {
        RuleFor(it => it).NotNull()
            .DependentRules(() => { RuleFor(it => it.NewName).NotEmpty().MaximumLength(50); });
    }
}