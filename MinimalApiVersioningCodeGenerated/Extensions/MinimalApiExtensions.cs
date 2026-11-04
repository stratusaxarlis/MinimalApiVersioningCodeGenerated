using System.Reflection;
using Asp.Versioning;
using MinimalApiVersioningCodeGenerated.Common.Interfaces;
using MinimalApiVersioningCodeGenerated.Common.Mappers;

namespace MinimalApiVersioningCodeGenerated.Extensions;

public static class CqrsEndpointExtensions
{
    public static IEndpointRouteBuilder MapCqrsEndpoints(this IEndpointRouteBuilder app, params Assembly[] assemblies)
    {
        var endpoints = CqrsEndpointDiscovery.Discover(assemblies);

        foreach (var byVersion in endpoints.GroupBy(e => e.Version).OrderBy(g => g.Key))
        {
            var versionedApi = app.NewVersionedApi();

            foreach (var byGroup in byVersion.GroupBy(e => e.Group))
            {
                var group = versionedApi
                    .MapGroup($"/api/v{{version:apiVersion}}/{byGroup.Key.ToLowerInvariant()}") // lowercased
                    .HasApiVersion(new ApiVersion(byVersion.Key))
                    .WithTags(byGroup.Key);

                foreach (var endpoint in byGroup)
                {
                    var mapper = (ICqrsEndpointMapper)Activator.CreateInstance(
                        typeof(CqrsEndpointMapper<>).MakeGenericType(endpoint.RequestType),
                        endpoint,
                        byVersion.Key)!;

                    mapper.Map(group);
                }
            }
        }

        return app;
    }
}
