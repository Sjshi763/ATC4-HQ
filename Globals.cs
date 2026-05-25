using System;
using System.Reflection;

namespace master
{
    namespace Globals
    {
        public static class GlobalPaths
        {
            public static string InitiatorProfileName = @".\ATC4-HQ.ini";
            public static string Version = GetAppVersion();
            public static string? TransitSoftwareLE;
            public static string? FirstRun;
            public static string Keys = "0x5A";
            public static string? GamePath;
            public static string? GameName;
            public static string LogPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ATC4-HQ", "logs");

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
