using MinimalApiVersioningCodeGenerated.Common.Enumerators;

namespace MinimalApiVersioningCodeGenerated.Common.Models.Options;

public record CqrsEndpoint(Type RequestType, string Route, HttpVerb Verb, int Version, int? UntilVersion, string Group, bool HasResponse);
