using System;
using System.IO;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using master.Globals;

namespace ATC4_HQ.ViewModels;

public partial class SettingViewModel : ViewModelBase
{
    [ObservableProperty]
    private MainWindowViewModel? _mainWindowViewModel;

    public ICommand OpenLogCommand => new RelayCommand(OpenLogDirectory);
    public ICommand OpenConfigDirectoryCommand => new RelayCommand(OpenConfigDirectory);
    public ICommand OpenGameConfigDirectoryCommand => new RelayCommand(OpenGameConfigDirectory);

    private void OpenLogDirectory()
    {
        OpenDirectory(GlobalPaths.LogPath, "日志目录");
    }

    private void OpenConfigDirectory()
    {
        var configDirectory = Path.GetDirectoryName(GlobalPaths.InitiatorProfileName) ?? AppPaths.CommonDirectory;
        OpenDirectory(configDirectory, "配置文件目录");
    }

    private void OpenGameConfigDirectory()
    {
        var gameDirectory = GlobalPaths.CurrentGame?.Path;
        if (string.IsNullOrWhiteSpace(gameDirectory))
        {
            LoggerHelper.LogWarning("打开游戏配置文件目录失败：当前未选择游戏。");
            return;
        }

        OpenDirectory(gameDirectory, "游戏配置文件目录");
    }

    private static void OpenDirectory(string directoryPath, string displayName)
    {
        try
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = directoryPath,
                UseShellExecute = true
            });
            LoggerHelper.LogInformation($"已打开{displayName}：{directoryPath}");
        }
        catch (Exception ex)
        {
            LoggerHelper.LogError($"打开{displayName}失败：{ex.Message}");
        }
    }
}
