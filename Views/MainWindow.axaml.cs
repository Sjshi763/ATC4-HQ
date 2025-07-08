using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Threading.Tasks;
using ATC4_HQ.ViewModels;
using System;
using System.Text.Json; // ⭐️ 新增：引入 JSON 命名空间
using ATC4_HQ.Models; // ⭐️ 新增：引入 GameModel 的命名空间
using master.Globals;
using System.IO;

namespace ATC4_HQ.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            StartUp();
        }

        private void InitializeComponent()
        {
            // 这个方法通常由 Avalonia.Markup.Xaml.AvaloniaXamlLoader.Load(this); 生成。
            // 如果您是手动添加的，请确保引用了 Avalonia.Markup.Xaml。
            Avalonia.Markup.Xaml.AvaloniaXamlLoader.Load(this);
        }

        private void MainWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            // 如果需要初始化 PageHost 的内容，可以在这里进行
            // 但更好的做法是让 ViewModel 的 CurrentPage 属性来控制
        }

        private void StartGame_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.StartGameCommand.Execute(null);
            }
        }

        // 安装游戏按钮的点击事件：打开对话框，将结果传递给 ViewModel
        private async void InstallGame_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel) // 确保 ViewModel 已设置
            {
                Console.WriteLine("安装游戏按钮被点击了，准备打开对话框。");

                var dialogWindow = new InstallGameDataDialogWindow();
                // 修复 CS8604 警告：确保 owner 是可空类型或显式转换
                Window? ownerWindow = TopLevel.GetTopLevel(this) as Window;
                bool? dialogResult = await dialogWindow.ShowDialog<bool?>(ownerWindow);

                // 现在获取的是 DialogResultData (JSON 字符串)
                if (dialogResult == true && dialogWindow.DataContext is InstallGameDataViewModel dialogViewModel)
                {
                    // ⭐️ 修复 CS1061 错误：使用 DialogResultData 而不是 DialogResultPath
                    string? selectedGameDataJson = dialogViewModel.DialogResultData;
                    if (!string.IsNullOrEmpty(selectedGameDataJson))
                    {
                        Console.WriteLine($"从对话框中获取到的 JSON 数据: {selectedGameDataJson}");
                        try
                        {
                            // ⭐️ 反序列化 JSON 字符串为 GameModel 对象
                            GameModel? gameData = JsonSerializer.Deserialize<GameModel>(selectedGameDataJson);
                            if (gameData != null)
                            {
                                Console.WriteLine($"解析到的游戏名称: {gameData.Name}, 路径: {gameData.Path}");
                                // ⭐️ 关键修改：调用 MainWindowViewModel 中的 HandleInstallGameAndUnzipAsync
                                await viewModel.HandleInstallGameAndUnzipAsync(gameData);
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

        private void Setting_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                Console.WriteLine("设置按钮被点击了！");
                viewModel.SettingCommand.Execute(null);
            }
        }

        private void StartUp()
        {

        }

        private string? ParseInformation(string SourcesOfInformation , string MessageName)
        {
            string[] lines = File.ReadAllLines(SourcesOfInformation);
            foreach (var line in lines)
            {
                if (line.Contains(MessageName))
                {
                    string Endline = line.Replace(MessageName + " = ", "");
                    return Endline;
                }
            }
            return null;
        }
    }
}