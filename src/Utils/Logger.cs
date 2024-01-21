using System.Reflection;

namespace Start.Utils;

internal enum LogLevel
{
    Debug,
    Information,
    Warn,
    Error,
    Fatal,
    Trace
}

internal enum LogType
{
    OnlyConsole,
    OnlyLogFile,
    ConsoleWithLogFile
}

internal static class Logger
{
    public static async Task LogAsync(object message, LogLevel logLevel = LogLevel.Information,
        LogType logType = LogType.OnlyConsole, string? logFilePath = default)
    {
        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name ?? string.Empty;
        if (logType is not LogType.OnlyLogFile)
        {
            Console.ForegroundColor = logLevel switch
            {
                LogLevel.Information => ConsoleColor.White,
                LogLevel.Warn => ConsoleColor.Yellow,
                LogLevel.Error => ConsoleColor.Red,
                LogLevel.Debug => ConsoleColor.Blue,
                LogLevel.Fatal => ConsoleColor.Gray,
                LogLevel.Trace => ConsoleColor.DarkGray,
                _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null)
            };
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss:fff} {logLevel}] [{assemblyName}] {message}");
            Console.ResetColor();
        }

        if (logType is LogType.OnlyConsole)
        {
            return;
        }

        string path = string.IsNullOrWhiteSpace(logFilePath) ? $"logs\\{DateTime.Now:yyyy-MM-dd}.log" : logFilePath;
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? string.Empty);
        await File.AppendAllLinesAsync(path, new[] { $"[{DateTime.Now} {logLevel}] [{assemblyName}] {message}" });
    }
}