using System;
using System.IO;

namespace master.Globals
{
    /// <summary>
    /// Provides a single place to resolve application-owned writable paths.
    ///
    /// Portable/debug runs keep common files next to the application (./). MSI-style
    /// installs are commonly placed under Program Files where normal users cannot
    /// write, so common files are redirected to %APPDATA%\ATC4-HQ.
    /// </summary>
    public static class AppPaths
    {
        private const string AppFolderName = "ATC4-HQ";

        /// <summary>
        /// Directory used for logs, configuration and other application-owned files.
        /// </summary>
        public static string CommonDirectory { get; } = ResolveCommonDirectory();

        /// <summary>
        /// Full path to the primary ini profile.
        /// </summary>
        public static string InitiatorProfilePath => System.IO.Path.Combine(CommonDirectory, "ATC4-HQ.ini");

        /// <summary>
        /// Full path to the log directory.
        /// </summary>
        public static string LogDirectory => System.IO.Path.Combine(CommonDirectory, "logs");

        private static string ResolveCommonDirectory()
        {
            var localDirectory = AppContext.BaseDirectory;

            if (!IsMsiStyleInstallDirectory(localDirectory) && CanWriteToDirectory(localDirectory))
            {
                return localDirectory;
            }

            return System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                AppFolderName);
        }

        private static bool IsMsiStyleInstallDirectory(string directory)
        {
            var normalizedDirectory = System.IO.Path.GetFullPath(directory)
                .TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);

            return IsUnderSpecialFolder(normalizedDirectory, Environment.SpecialFolder.ProgramFiles)
                   || IsUnderSpecialFolder(normalizedDirectory, Environment.SpecialFolder.ProgramFilesX86);
        }

        private static bool IsUnderSpecialFolder(string directory, Environment.SpecialFolder specialFolder)
        {
            var specialFolderPath = Environment.GetFolderPath(specialFolder);
            if (string.IsNullOrWhiteSpace(specialFolderPath))
            {
                return false;
            }

            var normalizedSpecialFolderPath = System.IO.Path.GetFullPath(specialFolderPath)
                .TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);

            return directory.Equals(normalizedSpecialFolderPath, StringComparison.OrdinalIgnoreCase)
                   || directory.StartsWith(
                       normalizedSpecialFolderPath + System.IO.Path.DirectorySeparatorChar,
                       StringComparison.OrdinalIgnoreCase)
                   || directory.StartsWith(
                       normalizedSpecialFolderPath + System.IO.Path.AltDirectorySeparatorChar,
                       StringComparison.OrdinalIgnoreCase);
        }

        private static bool CanWriteToDirectory(string directory)
        {
            try
            {
                Directory.CreateDirectory(directory);
                var testFile = System.IO.Path.Combine(directory, $".write-test-{Guid.NewGuid():N}.tmp");
                File.WriteAllText(testFile, string.Empty);
                File.Delete(testFile);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
