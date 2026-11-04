using Serilog.Sinks.SystemConsole.Themes;

namespace MinimalApiVersioningCodeGenerated.Constants;

public static class LoggingConstants
{
    private const string Reset = "\x1b[0m";

    // 256-color foreground: \x1b[38;5;{n}m   background: \x1b[48;5;{n}m
    private static string Fg(int n) => $"\x1b[38;5;{n}m";
    private static string Bg(int n) => $"\x1b[48;5;{n}m";

    public const string OutputTemplate =
        "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj} {Properties:j}{NewLine}{Exception}";

    public static readonly AnsiConsoleTheme ApiSerilogConsoleTheme = new(
        new Dictionary<ConsoleThemeStyle, string>
        {
            // message text
            [ConsoleThemeStyle.Text] = Fg(252),
            [ConsoleThemeStyle.SecondaryText] = Fg(245), // timestamp, source context
            [ConsoleThemeStyle.TertiaryText] = Fg(240), // brackets, separators
            [ConsoleThemeStyle.Invalid] = Fg(214),

            // structured property values
            [ConsoleThemeStyle.Null] = Fg(141),
            [ConsoleThemeStyle.Name] = Fg(110), // property names
            [ConsoleThemeStyle.String] = Fg(114),
            [ConsoleThemeStyle.Number] = Fg(215),
            [ConsoleThemeStyle.Boolean] = Fg(141),
            [ConsoleThemeStyle.Scalar] = Fg(180),

            // levels
            [ConsoleThemeStyle.LevelVerbose] = Fg(244),
            [ConsoleThemeStyle.LevelDebug] = Fg(75),
            [ConsoleThemeStyle.LevelInformation] = Fg(77),
            [ConsoleThemeStyle.LevelWarning] = Fg(220),
            [ConsoleThemeStyle.LevelError] = Fg(203),
            [ConsoleThemeStyle.LevelFatal] = Fg(231) + Bg(160)
        });
}