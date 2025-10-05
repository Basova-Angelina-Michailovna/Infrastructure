using FluentValidation;
using StretchRoom.Infrastructure.TestApplication.BoundedContext.Requests;

namespace StretchRoom.Infrastructure.TestApplication.Validators;

public class SomeBodyRequestValidator : AbstractValidator<SomeBodyRequest>
{
    public SomeBodyRequestValidator()
    {
        RuleFor(it => it).NotNull()
            .DependentRules(() => { RuleFor(it => it.Message).NotEmpty().MaximumLength(10); });
    }
}