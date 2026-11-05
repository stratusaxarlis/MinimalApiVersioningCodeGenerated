using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.Aspire;

var builder = DistributedApplication.CreateBuilder(args);
//IResourceBuilder<ProjectResource> api =
    builder.AddProject<Projects.MinimalApiVersioningCodeGenerated>("minimalapiversioningcodegenerated");
// Add API Reference
//api.WithExternalHttpEndpoints();
/*var scalar = builder.AddScalarApiReference();
// Register services with the API Reference
scalar
    .WithApiReference(api);*/

var app = builder.Build();
app.Run();
//var stoppingToken = app.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping;

//await app.RunAsync(stoppingToken);