using Avalonia.Controls;
using System;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Interactivity;
using ATC4_HQ.ViewModels;
using ATC4_HQ.Models; // 引入 GameModel 的命名空间
using System.Text.Json; // 用于 JSON 反序列化

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

        private async void OnRequestOpenInstallGameDataDialog(object? sender, EventArgs e)
        {
            var dialogWindow = new InstallGameDataDialogWindow();
            Window? ownerWindow = TopLevel.GetTopLevel(this) as Window;
            bool? dialogResult = await dialogWindow.ShowDialog<bool?>(ownerWindow);

            if (dialogResult == true && dialogWindow.DataContext is InstallGameDataViewModel dialogViewModel)
            {
                string? selectedGameDataJson = dialogViewModel.DialogResultData; // 确保使用 DialogResultData
                if (!string.IsNullOrEmpty(selectedGameDataJson))
                {
                    Console.WriteLine($"从对话框中获取到的 JSON 数据: {selectedGameDataJson}");
                    try
                    {
                        GameModel? gameData = JsonSerializer.Deserialize<GameModel>(selectedGameDataJson); // 反序列化为 GameModel
                        if (gameData != null)
                        {
                            Console.WriteLine($"解析到的游戏名称: {gameData.Name}, 路径: {gameData.Path}");

                            if (TopLevel.GetTopLevel(this) is Window mainWindow && mainWindow.DataContext is MainWindowViewModel mainWindowViewModel)
                            {
                                await mainWindowViewModel.HandleInstallGameAndUnzipAsync(gameData); // 确保传递 GameModel
                                Console.WriteLine("游戏数据已成功传递给 MainWindowViewModel 进行处理。");
                            }
                            else
                            {
                                Console.WriteLine("错误：无法获取 MainWindowViewModel 来处理游戏数据。");
                            }
                        }
                        else
                        {
                            Console.WriteLine("错误：无法将 JSON 数据反序列化为 GameModel 对象。");
                        }
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"JSON 反序列化错误: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"处理对话框结果时发生错误: {ex.Message}");
                    }
                }
            }
            else
            {
                Console.WriteLine("用户取消了选择或对话框关闭。");
            }
        }
    }
}