using MinimalApiVersioningCodeGenerated.Common.Enumerators;
using MinimalApiVersioningCodeGenerated.Common.Interfaces;

namespace MinimalApiVersioningCodeGenerated.Common.Models.Dto;

public sealed record Error(string Code, string Description, ErrorType Type)
{
	public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

	/// <summary>Per-field validation messages (property name -> messages); null for non-validation errors.</summary>
	public IReadOnlyDictionary<string, string[]>? Errors { get; init; }

	public static Error Failure(string code, string description) => new(code, description, ErrorType.Failure);
	public static Error Validation(string code, string description) => new(code, description, ErrorType.Validation);
	public static Error Validation(IReadOnlyDictionary<string, string[]> errors) =>
		new("Validation.Failed", "One or more validation errors occurred.", ErrorType.Validation) { Errors = errors };
	public static Error NotFound(string code, string description) => new(code, description, ErrorType.NotFound);
	public static Error Conflict(string code, string description) => new(code, description, ErrorType.Conflict);
	public static Error Unauthorized(string code, string description) => new(code, description, ErrorType.Unauthorized);
	public static Error Forbidden(string code, string description) => new(code, description, ErrorType.Forbidden);
}

/// <summary>Non-generic view used by the endpoint mapper to turn any result into an HTTP response.</summary>

public record Result : IResultBase, IResultFactory<Result>
{
	protected Result(bool isSuccess, Error error)
	{
		if (isSuccess && error != Error.None)
			throw new InvalidOperationException("A successful result cannot carry an error.");
		if (!isSuccess && error == Error.None)
			throw new InvalidOperationException("A failed result must carry an error.");

		IsSuccess = isSuccess;
		Error = error;
	}

	public bool IsSuccess { get; }
	public bool IsFailure => !IsSuccess;
	public Error Error { get; }
	public virtual object? GetValue() => null;
	public static Result Success() => new(true, Error.None);
	public static Result Failure(Error error) => new(false, error);
	protected static Result<T> Success<T>(T value) => new(value, true, Error.None);
	protected static Result<T> Failure<T>(Error error) => new(default, false, error);
	public static implicit operator Result(Error error) => Failure(error);
}

public sealed record Result<T> : Result, IResultFactory<Result<T>>
{
	private readonly T? _value;

	internal Result(T? value, bool isSuccess, Error error) : base(isSuccess, error) => _value = value;

	public static new Result<T> Failure(Error error) => Failure<T>(error);

	public T Value => IsSuccess
		? _value!
		: throw new InvalidOperationException("Cannot read the value of a failed result.");

	public override object? GetValue() => IsSuccess ? _value : null;

	public static implicit operator Result<T>(T value) => Success(value);
	public static implicit operator Result<T>(Error error) => Failure<T>(error);
}

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;

    public PagedResult<TOut> Map<TOut>(Func<T, TOut> map) => new(Items.Select(map).ToList(), Page, PageSize, TotalCount);

    public static PagedResult<T> Empty(int page, int pageSize) => new([], page, pageSize, 0);
}
