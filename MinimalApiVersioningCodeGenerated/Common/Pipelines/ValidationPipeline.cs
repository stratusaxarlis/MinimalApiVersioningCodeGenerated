using System.Text.Json;
using FluentValidation;
using MediatR;
using MinimalApiVersioningCodeGenerated.Common.Interfaces;
using MinimalApiVersioningCodeGenerated.Common.Models.Dto;

namespace MinimalApiVersioningCodeGenerated.Common.Pipelines;

/// <summary>
/// Runs every FluentValidation validator registered for the request before its handler.
/// Failures short-circuit into a validation <see cref="Result"/> (HTTP 400) instead of throwing.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : IResultFactory<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (!validators.Any())
            return await next(ct);

        var context = new ValidationContext<TRequest>(request);
        var results = await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, ct)));

        var errors = results
            .SelectMany(r => r.Errors)
            .GroupBy(f => JsonNamingPolicy.CamelCase.ConvertName(f.PropertyName)) // match the JSON field names clients send
            .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).Distinct().ToArray());

        if (errors.Count == 0)
            return await next(ct);

        return TResponse.Failure(Error.Validation(errors));
    }
}
