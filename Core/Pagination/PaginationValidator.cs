using FluentValidation;

namespace Core.Pagination;

public class PaginationValidator : AbstractValidator<PaginationQueryParams>
{
    public PaginationValidator()
    {
        RuleFor(x => x.Page)
            .NotEmpty()
            .GreaterThan(0);

        RuleFor(x => x.Limit)
            .NotEmpty()
            .GreaterThan(0);
    }
}
