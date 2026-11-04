using MinimalApiVersioningCodeGenerated.Constants;
using MinimalApiVersioningCodeGenerated.Extensions;
using MinimalApiVersioningCodeGenerated.Feature.TestFeature;
using Serilog;

LoggingExtensions.CreateBootstrapLogger();

try
{
	Log.Information("Starting {Application}", ApiConstants.DisplayName);

	var builder = WebApplication.CreateBuilder(args);

    builder.AddServiceDefaults();

	builder.AddSerilogLogging();

	builder.Services
		.AddApplication(typeof(Program).Assembly)
		.AddProductsFeature()
		.AddClientRateLimiting(builder.Configuration)
		.AddVersionedApi();

	var app = builder.Build();

    app.MapDefaultEndpoints();

	app.SeedProducts()
		.UseApiPipeline();

	app.MapCqrsEndpoints(typeof(Program).Assembly); // must run before MapApiDocumentation (DescribeApiVersions)
	app.MapApiDocumentation();

	await app.RunAsync();
	return 0;
}
catch (Exception ex) when (ex is not HostAbortedException)
{
	// HostAbortedException is thrown on purpose by tooling (EF design-time, build-time OpenAPI generation), not a real failure
	Log.Fatal(ex, "{Application} terminated unexpectedly", ApiConstants.DisplayName);
	return 1;
}
finally
{
	await Log.CloseAndFlushAsync();
}
