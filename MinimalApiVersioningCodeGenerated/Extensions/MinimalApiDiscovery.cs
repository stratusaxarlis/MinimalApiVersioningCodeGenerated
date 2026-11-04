using System.Reflection;
using MinimalApiVersioningCodeGenerated.Common.Attributes;
using MinimalApiVersioningCodeGenerated.Common.Interfaces;
using MinimalApiVersioningCodeGenerated.Common.Models.Options;

namespace MinimalApiVersioningCodeGenerated.Extensions;

public static class CqrsEndpointDiscovery
{
    private static readonly HashSet<Type> Markers = [typeof(ICommand), typeof(ICommand<>), typeof(IQuery<>)];

    public static IReadOnlyList<CqrsEndpoint> Discover(params Assembly[] assemblies)
        =>
        [
            .. assemblies.SelectMany(a => a.GetTypes())
                .Where(t => t is { IsClass: true, IsAbstract: false })
                .Select(t => (Type: t, Marker: t.GetInterfaces()
                    .Where(IsMarker)
                    .OrderByDescending(i => i.IsGenericType)
                    .FirstOrDefault()))
                .Where(x => x.Marker is not null)
                .Select(x =>
                {
                    var t = x.Type;
                    var attr = t.GetCustomAttribute<EndpointAttribute>()
                               ?? throw new InvalidOperationException($"{t.FullName} is missing [Endpoint].");
                    return new CqrsEndpoint(
                        t,
                        attr.Route,
                        attr.Verb,
                        attr.Version,
                        attr.UntilVersion,
                        attr.Group ?? InferGroup(t),
                        HasResponse: x.Marker!.IsGenericType);
                })
                .SelectMany(ExpandVersions)
        ];

    private static IEnumerable<CqrsEndpoint> ExpandVersions(CqrsEndpoint e)
    {
        int last = Math.Max(e.Version, e.UntilVersion ?? 0);   // unset (0 or null) means "only this version"
        for (int v = e.Version; v <= last; v++)
            yield return e with
            {
                Version = v,
                UntilVersion = null
            };
    }
    private static bool IsMarker(Type i) => Markers.Contains(i.IsGenericType ? i.GetGenericTypeDefinition() : i);
    private static string InferGroup(Type t)
    {
        string[] ns = t.Namespace?.Split('.') ?? [];
        int i = Array.IndexOf(ns, "Features");
        return i >= 0 && i + 1 < ns.Length ? ns[i + 1] : t.Name;
    }
}
