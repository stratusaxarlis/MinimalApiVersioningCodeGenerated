using MediatR;
using MinimalApiVersioningCodeGenerated.Common.Enumerators;
using MinimalApiVersioningCodeGenerated.Common.Interfaces;
using MinimalApiVersioningCodeGenerated.Common.Models.Options;
using MinimalApiVersioningCodeGenerated.Extensions;

namespace MinimalApiVersioningCodeGenerated.Common.Mappers;


public sealed class CqrsEndpointMapper<TRequest>(CqrsEndpoint definition, int version) : ICqrsEndpointMapper
	where TRequest : notnull
{
	public void Map(RouteGroupBuilder group)
	{
		RouteHandlerBuilder builder = definition.Verb switch
		{
			// GET/DELETE: bind from route + query string
			HttpVerb.Get    => group.MapGet(definition.Route, ParameterHandler),
			HttpVerb.Delete => group.MapDelete(definition.Route, ParameterHandler),
			// POST/PUT/PATCH: bind from JSON body
			HttpVerb.Post   => group.MapPost(definition.Route, BodyHandler),
			HttpVerb.Put    => group.MapPut(definition.Route, BodyHandler),
			HttpVerb.Patch  => group.MapPatch(definition.Route, BodyHandler),
			_ => throw new InvalidOperationException($"Unsupported verb {definition.Verb}")
		};

		builder
			.WithName($"{definition.Group}.{typeof(TRequest).Name}.v{version}")
			.WithSummary(typeof(TRequest).Name);

		// Handlers return IResult, so describe the responses for OpenAPI/Scalar explicitly
		if (definition.HasResponse)
			builder.Produces(StatusCodes.Status200OK, GetPayloadType());
		else
			builder.Produces(StatusCodes.Status204NoContent);

		builder
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.ProducesProblem(StatusCodes.Status409Conflict);
	}

	private async Task<IResult> BodyHandler(ISender sender, TRequest request)
		=> ToHttpResult(await sender.Send(request));

	private async Task<IResult> ParameterHandler(ISender sender, [AsParameters] TRequest request)
		=> ToHttpResult(await sender.Send(request));

	private IResult ToHttpResult(object? response) => response switch
	{
		IResultBase result => result.ToHttpResult(definition.HasResponse),
		_ => definition.HasResponse ? Results.Ok(response) : Results.NoContent()
	};

	// IQuery<T> / ICommand<T> -> T (the Result<T> payload type)
	private static Type? GetPayloadType() =>
		typeof(TRequest).GetInterfaces()
			.Where(i => i.IsGenericType &&
			            (i.GetGenericTypeDefinition() == typeof(IQuery<>) ||
			             i.GetGenericTypeDefinition() == typeof(ICommand<>)))
			.Select(i => i.GetGenericArguments()[0])
			.FirstOrDefault();
}
