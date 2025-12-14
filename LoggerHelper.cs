using Microsoft.Extensions.Logging;

namespace ATC4_HQ
{
    public static class LoggerHelper
    {
        private static ILogger _logger;

        public static void Initialize(ILogger logger)
        {
            _logger = logger;
        }

        public static ILogger Logger => _logger;
        
        public static void LogInformation(string message) => _logger.LogInformation(message);
        public static void LogWarning(string message) => _logger.LogWarning(message);
        public static void LogError(string message) => _logger.LogError(message);
        public static void LogDebug(string message) => _logger.LogDebug(message);
        public static void LogCritical(string message) => _logger.LogCritical(message);
    }
}
