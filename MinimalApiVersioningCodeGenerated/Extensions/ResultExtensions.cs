using MinimalApiVersioningCodeGenerated.Common.Enumerators;
using MinimalApiVersioningCodeGenerated.Common.Interfaces;
using MinimalApiVersioningCodeGenerated.Common.Models.Dto;

namespace MinimalApiVersioningCodeGenerated.Extensions;


public static class ResultHttpExtensions
{
    public static IResult ToHttpResult(this IResultBase result, bool hasResponse)
    {
        if (result.IsSuccess)
            return hasResponse ? Results.Ok(result.GetValue()) : Results.NoContent();

        return result.Error.ToProblem();
    }

    public static IResult ToProblem(this Error error)
    {
        (int status, string title) = error.Type switch
        {
            ErrorType.Validation   => (StatusCodes.Status400BadRequest, "Validation failed"),
            ErrorType.NotFound     => (StatusCodes.Status404NotFound, "Resource not found"),
            ErrorType.Conflict     => (StatusCodes.Status409Conflict, "Conflict"),
            ErrorType.Unauthorized => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            ErrorType.Forbidden    => (StatusCodes.Status403Forbidden, "Forbidden"),
            _                      => (StatusCodes.Status500InternalServerError, "Server error")
        };

        var extensions = new Dictionary<string, object?> { ["code"] = error.Code };

        if (error.Errors is { Count: > 0 } errors)
            return Results.ValidationProblem(
                new Dictionary<string, string[]>(errors),
                statusCode: status,
                title: title,
                detail: error.Description,
                extensions: extensions);

        return Results.Problem(
            statusCode: status,
            title: title,
            detail: error.Description,
            extensions: extensions);
    }
}
