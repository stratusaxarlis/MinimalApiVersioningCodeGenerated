using MinimalApiVersioningCodeGenerated.Constants;
using Scalar.AspNetCore;
using Serilog;

namespace MinimalApiVersioningCodeGenerated.Extensions;

public static class WebApplicationExtensions
{
	/// <summary>
	/// Configures the HTTP middleware pipeline. Order matters.
	/// </summary>
	public static WebApplication UseApiPipeline(this WebApplication app)
	{
		//app.UseForwardedHeaders(); use this if behind a reverse proxy (e.g. Nginx) to get the real client IP address for rate limiting
		app.UseSerilogRequestLogging(); // one structured log line per request
		app.UseRateLimiter();
		app.UseExceptionHandler();
		app.UseHttpsRedirection();

		return app;
	}

	/// <summary>
	/// Maps OpenAPI documents and the Scalar UI (Development only).
	/// Must run after the endpoints are mapped so DescribeApiVersions can see every version.
	/// </summary>
	public static WebApplication MapApiDocumentation(this WebApplication app)
	{
		if (!app.Environment.IsDevelopment())
			return app;

		app.MapOpenApi().WithDocumentPerVersion();

		app.MapScalarApiReference(options =>
		{
			options.WithTitle(ApiConstants.DocumentationTitle);
			options.WithTheme(ScalarTheme.BluePlanet);

			var descriptions = app.DescribeApiVersions();
			for (var i = 0; i < descriptions.Count; i++)
			{
				var d = descriptions[i];
				options.AddDocument(d.GroupName, d.GroupName, isDefault: i == descriptions.Count - 1);
			}
		});

		return app;
	}
}
