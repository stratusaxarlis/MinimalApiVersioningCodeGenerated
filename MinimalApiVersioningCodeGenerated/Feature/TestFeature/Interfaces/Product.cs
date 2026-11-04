namespace MinimalApiVersioningCodeGenerated.Feature.TestFeature.Interfaces;

// Product data shared by create requests across versions, so they share one set of validation rules
public interface IProductData
{
    string Name { get; }
    decimal Price { get; }
    string Currency { get; }
}
