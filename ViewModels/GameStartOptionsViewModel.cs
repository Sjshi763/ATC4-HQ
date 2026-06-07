using System;
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
    public ICommand Button3Command {  get; } // 启动选择游戏

    // 修改构造函数：接收 MainWindowViewModel 实例
    public GameStartOptionsViewModel(MainWindowViewModel mainWindowViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel; // 保存引用
        Button1Command = new RelayCommand(OnLaunchLastGame);
        Button3Command = new RelayCommand(OnListAllGames);
    }

    private void OnLaunchLastGame()
    {
        LoggerHelper.LogInformation("Game Start Options: 第一个按钮被点击了！尝试启动上一次游戏。");
            
    }

    private async void OnListAllGames()
    {
        LoggerHelper.LogInformation("Game Start Options: 第三个按钮被点击了！尝试启动选择的游戏。");
            
    }
}
