using System.Diagnostics;
using MinimalApiVersioningCodeGenerated.Constants;
using MinimalApiVersioningCodeGenerated.Extensions;
using MinimalApiVersioningCodeGenerated.Feature.TestFeature;
using Serilog;

LoggingExtensions.CreateBootstrapLogger();

try
{
    Log.Information("Starting {Application}", ApiConstants.DisplayName);

    var builder = WebApplication.CreateBuilder(args);

    // metrics, and logging providers are fully established in the dependency injection container.
    builder.AddServiceDefaults();

    // Serilog's writeToProviders: true can successfully pipe logs into Aspire.
    builder.AddSerilogLogging();

    builder.Services
        .AddApplication(typeof(Program).Assembly)
        .AddProductsFeature()
        .AddClientRateLimiting(builder.Configuration)
        .AddVersionedApi();

    var app = builder.Build();

    app.MapDefaultEndpoints();       // /health and /alive (Development only)
    app.UseSerilogRequestLogging(); 
    app.SeedProducts()
        .UseApiPipeline();

    app.MapCqrsEndpoints(typeof(Program).Assembly); // must run before MapApiDocumentation (DescribeApiVersions)
    app.MapApiDocumentation();

    app.MapGet("/", () => "ExampleApi is running. Try GET /api/v1/products");
    app.MapGet("/api/v1/boom", (ILogger<Program> logger) =>
    {
        logger.LogError("About to throw a demo exception");
        throw new InvalidOperationException("Demo exception to see an error trace");
    });
    
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

