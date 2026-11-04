using FluentValidation;
using MinimalApiVersioningCodeGenerated.Feature.TestFeature.EndPoints;

namespace MinimalApiVersioningCodeGenerated.Feature.TestFeature.Validators;

public sealed class ListProductsValidator: AbstractValidator<ListProducts>
{
    private const int MaxPageSize = 100;

    public ListProductsValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.Size)
            .InclusiveBetween(1, MaxPageSize);
    }
}
