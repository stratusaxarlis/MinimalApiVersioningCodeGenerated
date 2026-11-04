using System.Reflection;
using Asp.Versioning;
using FluentValidation;
using MinimalApiVersioningCodeGenerated.Common.Pipelines;
using MinimalApiVersioningCodeGenerated.Constants;

namespace MinimalApiVersioningCodeGenerated.Extensions;

public static class ServiceCollectionExtensions
{
	/// <summary>
	/// Registers the CQRS pipeline: MediatR handlers, FluentValidation validators and pipeline behaviors.
	/// </summary>
	public static IServiceCollection AddApplication(this IServiceCollection services, params Assembly[] assemblies)
	{
		services.AddValidatorsFromAssemblies(assemblies);
		services.AddMediatR(cfg =>
		{
			cfg.RegisterServicesFromAssemblies(assemblies);
			cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
		});

		return services;
	}

	/// <summary>
	/// Registers ProblemDetails, API versioning (URL segment + header) and per-version OpenAPI documents.
	/// </summary>
	public static IServiceCollection AddVersionedApi(this IServiceCollection services)
	{
		services.AddProblemDetails();
		services.AddApiVersioning(options =>
			{
				options.DefaultApiVersion = new ApiVersion(ApiConstants.DefaultApiVersion);
				options.AssumeDefaultVersionWhenUnspecified = true;
				options.ReportApiVersions = true;
				options.ApiVersionReader = ApiVersionReader.Combine(
					new UrlSegmentApiVersionReader(),
					new HeaderApiVersionReader(ApiConstants.ApiVersionHeader));
			})
			.AddApiExplorer(options =>
			{
				options.GroupNameFormat = ApiConstants.GroupNameFormat;
				options.SubstituteApiVersionInUrl = true;
			})
			.AddOpenApi();

		return services;
	}
}
