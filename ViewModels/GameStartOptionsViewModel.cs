using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using master.Globals;
using Microsoft.Extensions.Logging;

namespace ATC4_HQ.ViewModels;

public class GameStartOptionsViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _mainWindowViewModel;

    public string CurrentGameVersionText =>
        string.IsNullOrWhiteSpace(GlobalPaths.CurrentGame?.Name)
            ? "未选择游戏版本"
            : GlobalPaths.CurrentGame.Name;

    public IBrush AccentBrush => _mainWindowViewModel.AccentBrush;

    public IBrush AccentLightBrush => _mainWindowViewModel.AccentLightBrush;

    public ICommand Button1Command {  get; } // 启动上一次游戏
    public ICommand SelectGameCommand { get; } // 选择游戏

    // 修改构造函数：接收 MainWindowViewModel 实例
    public GameStartOptionsViewModel(MainWindowViewModel mainWindowViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel; // 保存引用
        Button1Command = new RelayCommand(OnLaunchLastGame);
        SelectGameCommand = new RelayCommand(OnSelectGame);
    }

    private void OnLaunchLastGame()
    {
        LoggerHelper.LogInformation("Game Start Options: 尝试启动上一次游戏。");

        var game = GlobalPaths.CurrentGame;
        if (game == null || string.IsNullOrWhiteSpace(game.Path))
        {
            LoggerHelper.LogWarning("未选择游戏，无法启动。");
            return;
        }

        var gameDirectory = Path.Combine(game.Path, "ATC4BKK");
        var executablePath = Path.Combine(gameDirectory, "AXA.exe");
        var titleArgument = Path.Combine("TITLE", "ATC4TITLE.axa");
        var titlePath = Path.Combine(gameDirectory, titleArgument);

        if (!Directory.Exists(gameDirectory))
        {
            LoggerHelper.LogError($"游戏目录不存在：{gameDirectory}");
            return;
        }

        if (!File.Exists(executablePath))
        {
            LoggerHelper.LogError($"启动程序不存在：{executablePath}");
            return;
        }

        if (!File.Exists(titlePath))
        {
            LoggerHelper.LogError($"标题文件不存在：{titlePath}");
            return;
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = executablePath,
            Arguments = @"TITLE\ATC4TITLE.axa",
            WorkingDirectory = gameDirectory,
            UseShellExecute = true
        };

        try
        {
            Process.Start(startInfo);
            LoggerHelper.LogInformation($"已启动游戏：{executablePath} TITLE\\ATC4TITLE.axa");
        }
        catch (Exception ex)
        {
            LoggerHelper.LogError($"启动游戏失败：{ex.Message}");
        }
    }

    private void OnSelectGame()
    {
        LoggerHelper.LogInformation("Game Start Options: 选择游戏按钮被点击了，在右边区域显示游戏列表。");
        _mainWindowViewModel.CurrentSubPage = new GameListViewModel();
    }
}
