using System.Collections.Generic;
using System.Reflection;

namespace master
{
    namespace Globals
    {
        public static class GlobalPaths
        {
            public static string InitiatorProfileName = AppPaths.InitiatorProfilePath;
            public static string Version = GetAppVersion();
            public static string? FirstRun;
            public static string Keys = "0x5A";
            public static string? GamePath;
            public static string? GameName;
            public static string LogPath = AppPaths.LogDirectory;
            public static string Atc4ArchiveBaseName = "ATC4";
            public static IReadOnlyList<string> RequiredAtc4ArchiveParts { get; } = new[]
            {
                "ATC4.z01",
                "ATC4.z02",
                "ATC4.z03",
                "ATC4.z04",
                "ATC4.z05",
                "ATC4.z06",
                "ATC4.z07",
                "ATC4.z08",
                "ATC4.zip"
            };

            private static string GetAppVersion()
            {
                var informationalVersion = Assembly.GetExecutingAssembly()
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    ?.InformationalVersion;

                if (!string.IsNullOrWhiteSpace(informationalVersion))
                {
                    return informationalVersion.Split('+')[0];
                }

                var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
                return assemblyVersion?.ToString() ?? "0.0.0";
            }
        }
    }
}
