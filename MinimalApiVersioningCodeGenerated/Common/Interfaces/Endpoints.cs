using MediatR;
using MinimalApiVersioningCodeGenerated.Common.Models.Dto;

namespace MinimalApiVersioningCodeGenerated.Common.Interfaces;

public interface ICommand : IRequest<Result>;
public interface ICommand<TResponse> : IRequest<Result<TResponse>>;
public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
public interface ICqrsEndpointMapper
{
    void Map(RouteGroupBuilder group);
}