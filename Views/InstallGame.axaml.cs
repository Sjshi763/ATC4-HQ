using Avalonia.Controls;
using System;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Interactivity;
using ATC4_HQ.ViewModels;
using ATC4_HQ.Models; // 引入 GameModel 的命名空间
using System.Text.Json; // 用于 JSON 反序列化
using Microsoft.Extensions.Logging;

namespace ATC4_HQ.Views
{
    public partial class InstallGame : UserControl
    {
        public InstallGame()
        {
            InitializeComponent();
            this.Loaded += InstallGame_Loaded;
            this.Unloaded += InstallGame_Unloaded;
        }

        private void InitializeComponent()
        {
            // 这个方法通常由 Avalonia.Markup.Xaml.AvaloniaXamlLoader.Load(this); 生成。
            // 如果您是手动添加的，请确保引用了 Avalonia.Markup.Xaml。
            Avalonia.Markup.Xaml.AvaloniaXamlLoader.Load(this);
        }

        private void InstallGame_Loaded(object? sender, RoutedEventArgs e)
        {
            if (DataContext is InstallGameViewModel viewModel)
            {
                viewModel.RequestOpenInstallGameDataDialog += OnRequestOpenInstallGameDataDialog;
            }
        }

        private void InstallGame_Unloaded(object? sender, RoutedEventArgs e)
        {
            if (DataContext is InstallGameViewModel viewModel)
            {
                viewModel.RequestOpenInstallGameDataDialog -= OnRequestOpenInstallGameDataDialog;
            }
        }

        private void OnRequestOpenInstallGameDataDialog(object? sender, EventArgs e)
        {
            // 直接在右边区域显示安装游戏数据界面，而不是弹出对话框
            if (TopLevel.GetTopLevel(this) is Window mainWindow && mainWindow.DataContext is MainWindowViewModel mainWindowViewModel)
            {
                var installGameDataViewModel = new InstallGameDataViewModel();
                
                // 订阅完成事件
                installGameDataViewModel.InstallGameDataCompleted += OnInstallGameDataCompleted;
                
                // 订阅清除右边区域事件
                installGameDataViewModel.ClearSubPageRequested += OnClearSubPageRequested;
                
                // 在右边区域显示
                mainWindowViewModel.CurrentSubPage = installGameDataViewModel;
                LoggerHelper.LogInformation("已在右边显示安装游戏数据界面。");
            }
        }

        private async void OnInstallGameDataCompleted(object? sender, InstallGameDataCompletedEventArgs e)
        {
            if (e.Success && e.GameData != null)
            {
                LoggerHelper.LogInformation($"获取到的游戏数据 - 名称: {e.GameData.Name}, 路径: {e.GameData.Path}");

                if (TopLevel.GetTopLevel(this) is Window mainWindow && mainWindow.DataContext is MainWindowViewModel mainWindowViewModel)
                {
                    await mainWindowViewModel.HandleInstallGameAndUnzipAsync(e.GameData);
                    LoggerHelper.LogInformation("游戏数据已成功传递给 MainWindowViewModel 进行处理。");
                }
                else
                {
                    LoggerHelper.LogError("错误：无法获取 MainWindowViewModel 来处理游戏数据。");
                }
            }
            else
            {
                LoggerHelper.LogInformation("用户取消了安装或安装失败。");
            }
        }

        private void OnClearSubPageRequested(object? sender, EventArgs e)
        {
            if (TopLevel.GetTopLevel(this) is Window mainWindow && mainWindow.DataContext is MainWindowViewModel mainWindowViewModel)
            {
                mainWindowViewModel.ClearSubPage();
                LoggerHelper.LogInformation("已清除右边区域的内容。");
            }
        }
    }
}
