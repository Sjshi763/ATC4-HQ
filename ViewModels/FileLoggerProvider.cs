using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace ATC4_HQ.ViewModels
{
    public class FileLoggerProvider : ILoggerProvider
    {
        private readonly string _logPath;
        private readonly LogLevel _minLevel;
        private readonly object _lock = new object();

        public FileLoggerProvider(string logPath, LogLevel minLevel)
        {
            _logPath = logPath;
            _minLevel = minLevel;

            if (!Directory.Exists(_logPath))
            {
                Directory.CreateDirectory(_logPath);
            }
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new FileLogger(categoryName, _logPath, _minLevel, _lock);
        }

        public void Dispose()
        {
        }
    }

    public class FileLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly string _logPath;
        private readonly LogLevel _minLevel;
        private readonly object _lock;

        public FileLogger(string categoryName, string logPath, LogLevel minLevel, object lockObj)
        {
            _categoryName = categoryName;
            _logPath = logPath;
            _minLevel = minLevel;
            _lock = lockObj;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= _minLevel;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var message = formatter(state, exception);
            var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{logLevel}] {_categoryName}: {message}";

            if (exception != null)
            {
                logEntry += Environment.NewLine + exception.ToString();
            }

            var logFile = Path.Combine(_logPath, $"log_{DateTime.Now:yyyy-MM-dd}.txt");

            lock (_lock)
            {
                try
                {
                    File.AppendAllText(logFile, logEntry + Environment.NewLine);
                }
                catch
                {
                }
            }
        }
    }

    public static class FileLoggerExtensions
    {
        public static ILoggingBuilder AddFile(this ILoggingBuilder builder, string logPath, LogLevel minLevel = LogLevel.Information)
        {
            builder.AddProvider(new FileLoggerProvider(logPath, minLevel));
            return builder;
        }
    }
}