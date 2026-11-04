using MinimalApiVersioningCodeGenerated.Constants;
using Serilog;
using Serilog.Events;

namespace MinimalApiVersioningCodeGenerated.Extensions;

public static class LoggingExtensions
{
	/// <summary>
	/// Bootstrap logger: captures failures that happen before the host is built.
	/// </summary>
	public static void CreateBootstrapLogger()
		=> Log.Logger = new LoggerConfiguration()
			.MinimumLevel.Override("Microsoft", LogEventLevel.Information)
			.Enrich.FromLogContext()
			.WriteTo.Console(outputTemplate: LoggingConstants.OutputTemplate, theme: LoggingConstants.ApiSerilogConsoleTheme)
			.CreateBootstrapLogger();

	/// <summary>
	/// Full logger: defaults first, then the appsettings "Serilog" section can override them.
	/// </summary>
	public static WebApplicationBuilder AddSerilogLogging(this WebApplicationBuilder builder)
	{
		builder.Host.UseSerilog((context, services, configuration) => configuration
			.MinimumLevel.Information()
			.MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
			.MinimumLevel.Override("System", LogEventLevel.Warning)
			.ReadFrom.Configuration(context.Configuration)
			.ReadFrom.Services(services)
			.Enrich.FromLogContext()
			.Enrich.WithProperty("Application", ApiConstants.ApplicationName)
			.Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
			.WriteTo.Console(outputTemplate: LoggingConstants.OutputTemplate, theme: LoggingConstants.ApiSerilogConsoleTheme));

		return builder;
	}
}
