using FluentValidation;

namespace Core.Sorting;

public class SortingValidator : AbstractValidator<SortingQueryParams>
{
    public SortingValidator()
    {
        When(x => x.Prop is not null || x.Dir is not null, () =>
        {
            RuleFor(x => x.Dir)
                .NotNull()
                .IsInEnum();

            RuleFor(x => x.Prop)
                .NotEmpty();
        });
    }
}
