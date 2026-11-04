using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using MinimalApiVersioningCodeGenerated.Common.Models.Options;
using MinimalApiVersioningCodeGenerated.Constants;

namespace MinimalApiVersioningCodeGenerated.Extensions;

public static class RateLimitingExtensions
{
	private const string NoLimit = "__no_limit__";

	public static IServiceCollection AddClientRateLimiting(this IServiceCollection services, IConfiguration configuration)
	{
		services.Configure<ClientRateLimitingOptions>(configuration.GetSection(RateLimiterConstants.SectionName));

		services.AddRateLimiter(_ => { });
		services.AddOptions<RateLimiterOptions>()
			.Configure<IOptions<ClientRateLimitingOptions>>((limiter, config) => Configure(limiter, config.Value));

		return services;
	}

	private static void Configure(RateLimiterOptions limiter, ClientRateLimitingOptions config)
	{
		limiter.RejectionStatusCode = config.HttpStatusCode;
		limiter.OnRejected = OnRejectedAsync;

		var endpointWhitelist = config.EndpointWhitelist.Select(EndpointMatcher.Parse).ToArray();
		var clientWhitelist = new HashSet<string>(config.ClientWhitelist, StringComparer.OrdinalIgnoreCase);

		// Every rule becomes its own limiter; the chain requires ALL of them to allow the request
		var limiters = config.GeneralRules
			.Where(r => config.EnableEndpointRateLimiting || IsGlobal(r))
			.Select(r => CreateRuleLimiter(r, config, endpointWhitelist, clientWhitelist))
			.ToArray();

		if (limiters.Length > 0)
			limiter.GlobalLimiter = PartitionedRateLimiter.CreateChained(limiters);
	}

	private static PartitionedRateLimiter<HttpContext> CreateRuleLimiter(
		RateLimitRule rule,
		ClientRateLimitingOptions config,
		EndpointMatcher[] endpointWhitelist,
		HashSet<string> clientWhitelist)
	{
		var matcher = EndpointMatcher.Parse(rule.Endpoint);
		var window = ParsePeriod(rule.Period);
		var isGlobal = IsGlobal(rule);
		var headerName = config.ClientIdHeader;
		var endpointRateLimiting = config.EnableEndpointRateLimiting;

		return PartitionedRateLimiter.Create<HttpContext, string>(context =>
		{
			var method = context.Request.Method;
			var path = context.Request.Path.Value ?? "/";

			if (!matcher.IsMatch(method, path) || endpointWhitelist.Any(w => w.IsMatch(method, path)))
				return RateLimitPartition.GetNoLimiter(NoLimit);

			var clientId = GetClientId(context, headerName);
			if (clientWhitelist.Contains(clientId))
				return RateLimitPartition.GetNoLimiter(NoLimit);

			// "*" rule, global mode        -> one counter per client
			// "*" rule, per-endpoint mode -> one counter per client + verb + path
			// specific rule               -> one counter per client + that rule's endpoint
			var key = isGlobal
				? endpointRateLimiting
					? $"{clientId}|{method.ToUpperInvariant()}:{EndpointMatcher.Normalize(path).ToLowerInvariant()}"
					: clientId
				: $"{clientId}|{rule.Endpoint.Trim().ToLowerInvariant()}";

			return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
			{
				PermitLimit = rule.Limit,
				Window = window,
				QueueLimit = 0,
				AutoReplenishment = true
			});
		});
	}

	private static async ValueTask OnRejectedAsync(OnRejectedContext context, CancellationToken cancellationToken)
	{
		var http = context.HttpContext;

		if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
			http.Response.Headers.RetryAfter = ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString(CultureInfo.InvariantCulture);

		var headerName = http.RequestServices.GetRequiredService<IOptions<ClientRateLimitingOptions>>().Value.ClientIdHeader;

		http.RequestServices.GetRequiredService<ILoggerFactory>()
			.CreateLogger("ClientRateLimiting")
			.LogWarning(
				"Rate limit exceeded for client {ClientId} on {Method} {Path}",
				GetClientId(http, headerName), http.Request.Method, http.Request.Path.Value);

		var problem = new ProblemDetails
		{
			Status = http.Response.StatusCode,
			Title = "Too many requests",
			Detail = "Rate limit exceeded. Try again later."
		};

		await http.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json", cancellationToken);
	}

	private static bool IsGlobal(RateLimitRule rule) => rule.Endpoint.Trim() == "*";

	private static string GetClientId(HttpContext context, string headerName)
	{
		var header = context.Request.Headers[headerName].ToString();
		if (!string.IsNullOrWhiteSpace(header))
			return header.Trim();

		return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
	}

	private static readonly Regex PeriodRegex = new(@"^(\d+)([smhd])$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

	private static TimeSpan ParsePeriod(string period)
	{
		var match = PeriodRegex.Match(period.Trim());
		if (!match.Success)
			throw new InvalidOperationException($"Invalid rate limit period '{period}'. Use a number followed by s, m, h or d (e.g. 15m).");

		var value = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
		return char.ToLowerInvariant(match.Groups[2].Value[0]) switch
		{
			's' => TimeSpan.FromSeconds(value),
			'm' => TimeSpan.FromMinutes(value),
			'h' => TimeSpan.FromHours(value),
			_ => TimeSpan.FromDays(value)
		};
	}

	private sealed class EndpointMatcher
	{
		private readonly string _verb;
		private readonly Regex _path;

		private EndpointMatcher(string verb, Regex path)
		{
			_verb = verb;
			_path = path;
		}

		public static EndpointMatcher Parse(string endpoint)
		{
			var raw = endpoint.Trim();

			if (raw == "*")
				return new EndpointMatcher("*", new Regex(".*", RegexOptions.Compiled));

			var separator = raw.IndexOf(':');
			var verb = separator > 0 ? raw[..separator] : "*";
			var path = Normalize(separator > 0 ? raw[(separator + 1)..] : raw);

			var pattern = "^" + Regex.Escape(path).Replace("\\*", ".*") + "$";
			var regex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

			return new EndpointMatcher(verb, regex);
		}

		public bool IsMatch(string method, string path) =>
			(_verb == "*" || string.Equals(_verb, method, StringComparison.OrdinalIgnoreCase))
			&& _path.IsMatch(Normalize(path));

		public static string Normalize(string path)
		{
			var trimmed = path.Length > 1 ? path.TrimEnd('/') : path;
			return trimmed.Length == 0 ? "/" : trimmed;
		}
	}
}
