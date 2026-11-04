using MediatR;
using MinimalApiVersioningCodeGenerated.Common.Interfaces;
using MinimalApiVersioningCodeGenerated.Common.Models.Dto;
using MinimalApiVersioningCodeGenerated.Feature.TestFeature.DTO;
using MinimalApiVersioningCodeGenerated.Feature.TestFeature.EndPoints;

namespace MinimalApiVersioningCodeGenerated.Feature.TestFeature.Queries;




public sealed class ListProductsHandler(IRepository<Product> repo): IRequestHandler<ListProducts, Result<PagedResult<ProductResponse>>>
{
    public async Task<Result<PagedResult<ProductResponse>>> Handle(ListProducts q, CancellationToken ct)
    {
        var page = await repo.GetPagedAsync(q.Page, q.Size, ct: ct);
        return page.Map(p => new ProductResponse(p.Id, p.Name, p.Price, p.Currency));
    }
}
