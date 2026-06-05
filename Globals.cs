using System.Collections.Generic;
using System.Reflection;
using Microsoft.Win32;
using ATC4_HQ.Models;

// 这个文件只保存在全局需要用到的常量和路径

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
            public static List<GameModel> Games = new List<GameModel>();
            public static GameModel? CurrentGame;
            public static string LogPath = AppPaths.LogDirectory;
            public static string Atc4ArchiveBaseName = "ATC4";
            public static class AccentColor
            {
                /// <summary>
                /// Windows 个性化主色调（从注册表读取 AccentColor）。
                /// 格式为 "#RRGGBB"，默认回退 "#2196F3"。
                /// </summary>
                public static string main = GetWindowsAccentColor();

                /// <summary>
                /// 主色调的浅色变体，用于辅助标签等。
                /// 默认回退 "#4CAF50"。
                /// </summary>
                public static string Light = GetWindowsAccentColorLight();
            }
            private static string GetWindowsAccentColor()
            {
                try
                {
                    using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\DWM");
                    if (key != null)
                    {
                        var value = key.GetValue("AccentColor");
                        if (value is int dword)
                        {
                            // AccentColor 是 ABGR 格式的 DWORD：0xAABBGGRR
                            int r = dword & 0xFF;
                            int g = (dword >> 8) & 0xFF;
                            int b = (dword >> 16) & 0xFF;
                            return $"#{r:X2}{g:X2}{b:X2}";
                        }
                    }
                }
                catch
                {
                    // 读取失败时使用默认值
                }
                return "#2196F3";
            }

            private static string GetWindowsAccentColorLight()
            {
                try
                {
                    using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\DWM");
                    if (key != null)
                    {
                        var value = key.GetValue("AccentColor");
                        if (value is int dword)
                        {
                            // 从主色调生成浅色变体：混合白色 50%
                            int r = dword & 0xFF;
                            int g = (dword >> 8) & 0xFF;
                            int b = (dword >> 16) & 0xFF;
                            int lr = (r + 255) / 2;
                            int lg = (g + 255) / 2;
                            int lb = (b + 255) / 2;
                            return $"#{lr:X2}{lg:X2}{lb:X2}";
                        }
                    }
                }
                catch
                {
                    // 读取失败时使用默认值
                }
                return "#4CAF50";
            }
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
