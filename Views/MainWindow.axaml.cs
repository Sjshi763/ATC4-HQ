using Avalonia.Controls;
using Avalonia.Interactivity;
using ATC4_HQ.ViewModels;
using System;
using System.Text.Json; // ⭐️ 新增：引入 JSON 命名空间
using ATC4_HQ.Models; // ⭐️ 新增：引入 GameModel 的命名空间
using master.Globals;
using System.IO;
using System.Text;
using Masuit.Tools.Security;
using Masuit.Tools.Files;
using Avalonia.Layout; // 用于布局相关类
using Avalonia.Media; // 用于媒体相关类
using Avalonia; // 用于Thickness等类

namespace ATC4_HQ.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
            StartUp();
            
            // 订阅DataContextChanged事件，确保在DataContext设置后再订阅OpenALNotInstalled事件
            this.DataContextChanged += OnDataContextChanged;
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
            try
            {
                if (!File.Exists(GlobalPaths.InitiatorProfileName))
                {
                    Console.WriteLine("错误：配置文件不存在，请检查路径。");
                    PrimaryProfile();
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"错误：无法访问配置文件 - {ex.Message}");
                return;
            }
            Console.WriteLine("配置文件存在，正在加载...");
            IniFile ini = new IniFile(GlobalPaths.InitiatorProfileName);
            var PrimaryProfileVersion = ini.GetValue("main", "Version");
            if (PrimaryProfileVersion != GlobalPaths.Version)
            {
                Console.WriteLine($"配置文件版本不匹配，当前版本：{GlobalPaths.Version}，配置文件版本：{PrimaryProfileVersion}");
                return;
            }
            GlobalPaths.TransitSoftwareLE = ini.GetValue("main", "TransitSoftwareLE");
            GlobalPaths.GamePath = ini.GetValue("main", "GamePath");
        }

        private void PrimaryProfile()
        {
            // 获取时间
            Console.WriteLine("配置文件不存在，正在创建初始配置文件...");
            DateTimeOffset now = DateTimeOffset.UtcNow;
            Console.WriteLine($"当前时间（UTC）：{now}");
            string generalShort = now.ToString("g");
            Console.WriteLine($"当前时间：{generalShort}");

            // 获取加密时间
            string encryptedText = generalShort.AESEncrypt(GlobalPaths.Keys);;
            Console.WriteLine($"加密后的时间：{encryptedText}");

            //返回值
            GlobalPaths.FirstRun = encryptedText;

            Console.WriteLine("创建初始配置文件...");
            IniFile ini=new IniFile(GlobalPaths.InitiatorProfileName);
            ini.SetValue("main", "Version", GlobalPaths.Version);
            ini.SetValue("main", "FirstRun", GlobalPaths.FirstRun);
            ini.SetValue("main", "TransitSoftwareLE" , "null");
            Console.WriteLine("初始配置文件已创建。");
            ini.Save();
        }
        
        /// <summary>
        /// 处理DataContextChanged事件，订阅OpenALNotInstalled事件
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        private void OnDataContextChanged(object? sender, EventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.OpenALNotInstalled += OnOpenALNotInstalled;
            }
        }
        
        /// <summary>
        /// 处理OpenAL未安装事件，显示警告对话框
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        private async void OnOpenALNotInstalled(object? sender, EventArgs e)
        {
            // 创建警告对话框
            var warningWindow = new Window
            {
                Title = "警告",
                Width = 450,
                Height = 250,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                CanResize = false
            };

            var stackPanel = new StackPanel { Margin = new Thickness(20) };

            var titleTextBlock = new TextBlock
            {
                Text = "警告",
                FontSize = 24,
                FontWeight = FontWeight.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };

            var contentTextBlock = new TextBlock
            {
                Text = "系统未检测到OPENAL，请先安装OPENAL库后再运行程序。",
                FontSize = 18,
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 30)
            };

            // 创建按钮面板
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Spacing = 20
            };

            var installButton = new Button
            {
                Content = "前往安装",
                Width = 120,
                Height = 40,
                FontSize = 16
            };

            var cancelButton = new Button
            {
                Content = "取消启动",
                Width = 120,
                Height = 40,
                FontSize = 16
            };

            // 设置按钮点击事件
            bool shouldInstall = false;
            installButton.Click += (s, args) =>
            {
                shouldInstall = true;
                warningWindow.Close();
            };

            cancelButton.Click += (s, args) =>
            {
                warningWindow.Close();
            };

            buttonPanel.Children.Add(installButton);
            buttonPanel.Children.Add(cancelButton);

            stackPanel.Children.Add(titleTextBlock);
            stackPanel.Children.Add(contentTextBlock);
            stackPanel.Children.Add(buttonPanel);

            warningWindow.Content = stackPanel;
            
            // 显示警告对话框
            await warningWindow.ShowDialog(this);
            
            // 如果用户选择前往安装，可以在这里添加安装逻辑
            if (shouldInstall)
            {
                // 这里可以添加打开安装页面或执行安装的逻辑
                Console.WriteLine("用户选择前往安装OPENAL");
                // 例如：打开浏览器到OPENAL下载页面，或者显示安装界面
            }
        }
    }
}
