using System;
using System.IO;

namespace ATC4_HQ.ViewModels
{
    /// <summary>
    /// Provides a single place to resolve application-owned writable paths.
    /// </summary>
    public static class AppPaths
    {
        private const string AppFolderName = "ATC4-HQ";

        public static string CommonDirectory { get; } = ResolveCommonDirectory();

        public static string InitiatorProfilePath => System.IO.Path.Combine(CommonDirectory, "ATC4-HQ.ini");

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