﻿using Microsoft.Extensions.Logging;
using System;
using DSharpPlus;
using DSharpPlus.Exceptions;

namespace SCPDiscord;

internal class LogTestFactory : ILoggerProvider
{
    public void Dispose() { /* nothing to dispose */ }

    public ILogger CreateLogger(string categoryName)
    {
        return Logger.instance;
    }
}

public class Logger : ILogger
{
    public static Logger instance { get; } = new Logger();

    private static LogLevel minimumLogLevel = LogLevel.Trace;
    private readonly object @lock = new();

    internal static void SetLogLevel(LogLevel level)
    {
        minimumLogLevel = level;
    }

    internal static void Debug(string message, Exception exception = null)
    {
        instance.Log(LogLevel.Debug, new EventId(420, "BOT"), exception, message);
    }

    internal static void Log(string message, Exception exception = null)
    {
        instance.Log(LogLevel.Information, new EventId(420, "BOT"), exception, message);
    }

    internal static void Warn(string message, Exception exception = null)
    {
        instance.Log(LogLevel.Warning, new EventId(420, "BOT"), exception, message);
    }

    internal static void Error(string message, Exception exception = null)
    {
        instance.Log(LogLevel.Error, new EventId(420, "BOT"), exception, message);
    }

    internal static void Fatal(string message, Exception exception = null)
    {
        instance.Log(LogLevel.Critical, new EventId(420, "BOT"), exception, message);
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= minimumLogLevel && logLevel != LogLevel.None;
    }

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => default;

    private static ConsoleColor GetLogLevelColour(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace       => ConsoleColor.White,
            LogLevel.Debug       => ConsoleColor.DarkGray,
            LogLevel.Information => ConsoleColor.DarkBlue,
            LogLevel.Warning     => ConsoleColor.Yellow,
            LogLevel.Error       => ConsoleColor.Red,
            _                    => ConsoleColor.White
        };
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        // Ratelimit messages are usually warnings, but they are unimportant in this case so downgrade them to debug.
        if (formatter(state, exception).StartsWith("Hit Discord ratelimit on route ") && logLevel == LogLevel.Warning)
        {
            logLevel = LogLevel.Debug;
        }
        // The bot will handle NotFoundExceptions on its own, downgrade to debug
        else if (exception is NotFoundException && eventId == LoggerEvents.RestError)
        {
            logLevel = LogLevel.Debug;
        }
        // I haven't found any way to catch this issue so I guess I will just look for the log message?
        else if (formatter(state, exception).StartsWith("An attempt to reconnect upon error code 4014 failed."))
        {
            Fatal("Bot intents are not set up correctly. Fix this and then restart the bot.");
            Environment.Exit(14);
        }

        string[] logLevelParts = logLevel switch
        {
            LogLevel.Trace       => ["[", "Trace", "] "],
            LogLevel.Debug       => ["[", "Debug", "] "],
            LogLevel.Information => [" [", "Info", "] "],
            LogLevel.Warning     => [" [", "Warn", "] "],
            LogLevel.Error       => ["[", "Error", "] "],
            LogLevel.Critical    => [" [", "\u001b[1mCrit\u001b[0m", "] "],
            _                    => [" [", "None", "] "],
        };

        lock (@lock)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("[");

            Console.ResetColor();
            Console.ForegroundColor = GetLogLevelColour(logLevel);
            if (logLevel == LogLevel.Critical)
            {
                Console.BackgroundColor = ConsoleColor.DarkRed;
            }
            Console.Write($"{DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("] ");

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("[");

            Console.ForegroundColor = eventId == 420 ? ConsoleColor.Green : ConsoleColor.DarkGreen;
            Console.Write(eventId == 420 ? "BOT" : "API");

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("] ");
            Console.Write(logLevelParts[0]);

            Console.ForegroundColor = GetLogLevelColour(logLevel);
            if (logLevel == LogLevel.Critical)
            {
                Console.BackgroundColor = ConsoleColor.DarkRed;
            }
            Console.Write(logLevelParts[1]);

            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(logLevelParts[2]);

            Console.ResetColor();
            if (logLevel is LogLevel.Trace or LogLevel.Debug)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            else if (logLevel is LogLevel.Critical or LogLevel.Error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            Console.WriteLine(formatter(state, exception));

            if (exception != null)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"{exception} : {exception.Message}\n{exception.StackTrace}");
            }

            Console.ResetColor();
        }
    }
}