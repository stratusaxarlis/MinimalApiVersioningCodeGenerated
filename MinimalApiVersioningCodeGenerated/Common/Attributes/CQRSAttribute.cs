using MinimalApiVersioningCodeGenerated.Common.Enumerators;

namespace MinimalApiVersioningCodeGenerated.Common.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class EndpointAttribute(string route) : Attribute
{
    public string Route { get; } = route;
    public HttpVerb Verb { get; init; } = HttpVerb.Post;
    public int Version { get; init; } = 1;
    public int UntilVersion { get; init; }   // inclusive: endpoint lives through this version
    public string? Group { get; init; }
}
