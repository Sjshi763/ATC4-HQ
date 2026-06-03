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
    private MainWindowViewModel _mainWindowViewModel;

    public ICommand OpenLogCommand => new RelayCommand(OpenLogDirectory);

    private void OpenLogDirectory()
    {
        try
        {
            if (!Directory.Exists(GlobalPaths.LogPath))
            {
                Directory.CreateDirectory(GlobalPaths.LogPath);
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = GlobalPaths.LogPath,
                UseShellExecute = true
            });
            LoggerHelper.LogInformation($"已打开日志目录：{GlobalPaths.LogPath}");
        }
        catch (Exception ex)
        {
            LoggerHelper.LogError($"打开日志目录失败：{ex.Message}");
        }
    }
}
