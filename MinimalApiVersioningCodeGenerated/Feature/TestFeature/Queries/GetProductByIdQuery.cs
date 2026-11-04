using MediatR;
using MinimalApiVersioningCodeGenerated.Common.Interfaces;
using MinimalApiVersioningCodeGenerated.Common.Models.Dto;
using MinimalApiVersioningCodeGenerated.Feature.TestFeature.DTO;
using MinimalApiVersioningCodeGenerated.Feature.TestFeature.EndPoints;
using MinimalApiVersioningCodeGenerated.Feature.TestFeature.Errors;

namespace MinimalApiVersioningCodeGenerated.Feature.TestFeature.Queries;

public sealed class GetProductByIdHandler(IRepository<Product> repo): IRequestHandler<GetProductById, Result<ProductResponse>>
{
    public async Task<Result<ProductResponse>> Handle(GetProductById q, CancellationToken ct)
    {
        var p = await repo.GetByIdAsync(q.Id, ct);
        if (p is null)
            return ProductErrors.NotFound(q.Id);

        return new ProductResponse(p.Id, p.Name, p.Price, p.Currency);
    }
}

public sealed class GetProductByIdV2Handler(IRepository<Product> repo): IRequestHandler<GetProductByIdV2, Result<ProductResponseV2>>
{
    public async Task<Result<ProductResponseV2>> Handle(GetProductByIdV2 q, CancellationToken ct)
    {
        var p = await repo.GetByIdAsync(q.Id, ct);
        if (p is null)
            return ProductErrors.NotFound(q.Id);

        return new ProductResponseV2(p.Id, p.Name, new MoneyResponse(p.Price, p.Currency));
    }
}