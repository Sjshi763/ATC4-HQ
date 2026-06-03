using System.Reflection;

namespace master
{
    namespace Globals
    {
        public static class GlobalPaths
        {
            public static string InitiatorProfileName = AppPaths.InitiatorProfilePath;
            public static string Version = GetAppVersion();
            public static string? TransitSoftwareLE;
            public static string? FirstRun;
            public static string Keys = "0x5A";
            public static string? GamePath;
            public static string? GameName;
            public static string LogPath = AppPaths.LogDirectory;

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
