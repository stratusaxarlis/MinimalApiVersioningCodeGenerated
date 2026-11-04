using FluentValidation;
using MinimalApiVersioningCodeGenerated.Feature.TestFeature.EndPoints;

namespace MinimalApiVersioningCodeGenerated.Feature.TestFeature.Validators;


public sealed class CreateProductValidator: AbstractValidator<CreateProduct>
{
    public CreateProductValidator() => Include(new CommonProductValidator());
}

public sealed class CreateProductV2Validator: AbstractValidator<CreateProductV2>
{
    public CreateProductV2Validator() => Include(new CommonProductValidator());
}

