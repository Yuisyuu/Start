﻿using System.Reflection;

namespace Start.Utils;

public static class Logger
{
    public enum LogLevel
    {
        DEBUG = 0,
        INFO = 1,
        WARN = 2,
        ERROR = 3,
        FATAL = 4
    }
    public enum LogType
    {
        OnlyConsole = 0,
        OnlyLogFile = 1,
        ConsoleWithLogFile = 2
    }
    public static void Log(object message, LogLevel logLevel = LogLevel.INFO, LogType logType = LogType.OnlyConsole, string logFilePath = null)
    {
        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name ?? string.Empty;
        if (logType != LogType.OnlyLogFile)
        {
            switch (logLevel)
            {
                case LogLevel.INFO:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogLevel.WARN:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogLevel.ERROR:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogLevel.DEBUG:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;
            }
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss:fff} {logLevel}] [{assemblyName}] {message}");
            Console.ResetColor();
        }
        if (logType == LogType.OnlyConsole)
        {
            return;
        }
        string path = string.IsNullOrWhiteSpace(logFilePath) ? $"logs\\{DateTime.Now:yyyy-MM-dd}.log" : logFilePath;
        _ = Directory.CreateDirectory(Path.GetDirectoryName(path) ?? string.Empty);
        File.AppendAllLines(path, new[] { $"[{DateTime.Now} {logLevel}] [{assemblyName}] {message}" });
    }
}