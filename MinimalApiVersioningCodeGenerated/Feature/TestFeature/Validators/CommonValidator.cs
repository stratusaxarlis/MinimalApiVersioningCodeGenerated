using FluentValidation;
using MinimalApiVersioningCodeGenerated.Feature.TestFeature.Interfaces;

namespace MinimalApiVersioningCodeGenerated.Feature.TestFeature.Validators;

// Shared rules for every request that carries product data (v1 + v2 create).
// Internal so assembly scanning doesn't register it on its own; it is pulled in via Include().
internal sealed class CommonProductValidator: AbstractValidator<IProductData>
{
    public CommonProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Price)
            .GreaterThan(0);

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Matches("^[A-Z]{3}$")
            .WithMessage("'{PropertyName}' must be a 3-letter ISO 4217 code (e.g. 'USD').");
    }
}
