using MediatR;
using MinimalApiVersioningCodeGenerated.Common.Interfaces;
using MinimalApiVersioningCodeGenerated.Common.Models.Dto;
using MinimalApiVersioningCodeGenerated.Feature.TestFeature.DTO;
using MinimalApiVersioningCodeGenerated.Feature.TestFeature.EndPoints;
using MinimalApiVersioningCodeGenerated.Feature.TestFeature.Errors;

namespace MinimalApiVersioningCodeGenerated.Feature.TestFeature.Commands;

public sealed class DeleteProductHandler(IRepository<Product> repo): IRequestHandler<DeleteProduct, Result>
{
    public async Task<Result> Handle(DeleteProduct cmd, CancellationToken ct)
    {
        if (!await repo.DeleteAsync(cmd.Id, ct))
            return ProductErrors.NotFound(cmd.Id);

        return Result.Success();
    }
}
