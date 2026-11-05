using MinimalApiVersioningCodeGenerated.Constants;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.OpenTelemetry; // <-- Required for direct OTLP export

namespace MinimalApiVersioningCodeGenerated.Extensions;

public static class LoggingExtensions
{
    public static void CreateBootstrapLogger()
        => Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateBootstrapLogger();

    public static WebApplicationBuilder AddSerilogLogging(this WebApplicationBuilder builder)
    {
        // TURN ON SELF-LOGGING: If Serilog fails to talk to Aspire, it will print the exact error to your console
        Serilog.Debugging.SelfLog.Enable(msg => Console.WriteLine($"[Serilog SelfLog] {msg}"));

        builder.Host.UseSerilog((context, services, configuration) => configuration
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", ApiConstants.ApplicationName)
            .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
            .WriteTo.Console(outputTemplate: LoggingConstants.OutputTemplate, theme: LoggingConstants.ApiSerilogConsoleTheme)
         ,writeToProviders:true  
        );

        return builder;
    }
}