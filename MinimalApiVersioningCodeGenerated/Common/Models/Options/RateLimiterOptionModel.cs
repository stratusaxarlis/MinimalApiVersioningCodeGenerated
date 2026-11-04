namespace MinimalApiVersioningCodeGenerated.Common.Models.Options;


public sealed record ClientRateLimitingOptions
{
    /// <summary>
    /// true  = every rule is counted per endpoint (client + verb + path for "*" rules, client + rule endpoint for specific rules).
    /// false = only "*" rules apply, counted globally per client.
    /// </summary>
    public bool EnableEndpointRateLimiting { get; set; }

    /// <summary>Read for compatibility but not supported by the built-in limiter (see notes).</summary>
    public bool StackBlockedRequests { get; set; }

    public string ClientIdHeader { get; set; } = "x-client-ip";
    public int HttpStatusCode { get; set; } = StatusCodes.Status429TooManyRequests;

    /// <summary>Entries like "get:/api/license", "*:/api/status", "*:/signalRHub". '*' works as a wildcard in the path too.</summary>
    public List<string> EndpointWhitelist { get; set; } = [];

    public List<string> ClientWhitelist { get; set; } = [];
    public List<RateLimitRule> GeneralRules { get; set; } = [];
}

public sealed record RateLimitRule
{
    /// <summary>"*" or "verb:/path", e.g. "post:/identity/sign-in".</summary>
    public string Endpoint { get; set; } = "*";

    /// <summary>Number followed by s, m, h or d, e.g. "1s", "15m", "12h", "7d".</summary>
    public string Period { get; set; } = "1s";

    public int Limit { get; set; }
}
