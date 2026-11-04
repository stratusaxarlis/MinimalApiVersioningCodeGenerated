using MinimalApiVersioningCodeGenerated.Common.Models.Dto;

namespace MinimalApiVersioningCodeGenerated.Feature.TestFeature.Errors;

public static class ProductErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("Product.NotFound", $"Product with id '{id}' was not found.");
}
