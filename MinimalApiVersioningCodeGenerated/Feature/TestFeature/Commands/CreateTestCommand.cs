using MediatR;
using MinimalApiVersioningCodeGenerated.Common.Interfaces;
using MinimalApiVersioningCodeGenerated.Common.Models.Dto;
using MinimalApiVersioningCodeGenerated.Feature.TestFeature.DTO;
using MinimalApiVersioningCodeGenerated.Feature.TestFeature.EndPoints;

namespace MinimalApiVersioningCodeGenerated.Feature.TestFeature.Commands;


public sealed class CreateProductHandler(IRepository<Product> repo): IRequestHandler<CreateProduct, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateProduct cmd, CancellationToken ct)
    {
        var p = new Product(Guid.NewGuid(), cmd.Name.Trim(), cmd.Price, cmd.Currency);
        await repo.AddAsync(p, ct);
        return p.Id;
    }
}

public sealed class CreateProductV2Handler(IRepository<Product> repo): IRequestHandler<CreateProductV2, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateProductV2 cmd, CancellationToken ct)
    {
        var p = new Product(Guid.NewGuid(), cmd.Name.Trim(), cmd.Price, cmd.Currency);
        await repo.AddAsync(p, ct);
        return p.Id;
    }
}
