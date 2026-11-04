var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.MinimalApiVersioningCodeGenerated>("minimalapiversioningcodegenerated");

builder.Build().RunAsync();
