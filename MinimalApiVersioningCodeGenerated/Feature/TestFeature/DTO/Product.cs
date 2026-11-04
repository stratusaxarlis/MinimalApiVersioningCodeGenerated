using MinimalApiVersioningCodeGenerated.Common.Interfaces;

namespace MinimalApiVersioningCodeGenerated.Feature.TestFeature.DTO;

public sealed record Product(Guid Id, string Name, decimal Price, string Currency): IEntity;