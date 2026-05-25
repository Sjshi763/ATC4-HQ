using Avalonia.Controls;
using Avalonia.Interactivity;
using ATC4_HQ.ViewModels;
using System;
using System.Text.Json; // ⭐️ 新增：引入 JSON 命名空间
using ATC4_HQ.Models; // ⭐️ 新增：引入 GameModel 的命名空间
using master.Globals;
using System.IO;
using System.Text;
using SoftCircuits.IniFileParser;
using System.Threading.Tasks; // 添加Task支持
using Avalonia;
using Avalonia.Media;
using Avalonia.Input;
using Microsoft.Extensions.Logging;
using System.Net.Http;

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
            
            // 订阅DataContextChanged事件，确保在DataContext设置后再订阅OpenALNotInstalled事件
            this.DataContextChanged += OnDataContextChanged;
            
            // 初始化窗口拖拽和按钮事件
            InitializeWindowControls();
        }

        private void InitializeComponent()
        {
            // 这个方法通常由 Avalonia.Markup.Xaml.AvaloniaXamlLoader.Load(this); 生成。
            // 如果您是手动添加的，请确保引用了 Avalonia.Markup.Xaml。
            Avalonia.Markup.Xaml.AvaloniaXamlLoader.Load(this);
        }
        
        /// <summary>
        /// 初始化窗口控件事件
        /// </summary>
        private void InitializeWindowControls()
        {
            // 查找最小化和关闭按钮
            var minButton = this.Find<Button>("BtnTitleMin");
            var closeButton = this.Find<Button>("BtnTitleClose");
            
            if (minButton != null)
            {
                minButton.Click += (s, e) => this.WindowState = WindowState.Minimized;
            }
            
            if (closeButton != null)
            {
                closeButton.Click += (s, e) => this.Close();
            }
            
            // 设置窗口拖拽
            this.PointerPressed += OnWindowPointerPressed;
        }
        
        /// <summary>
        /// 处理窗口拖拽
        /// </summary>
        private void OnWindowPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                this.BeginMoveDrag(e);
            }
        }

        private async void MainWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            // 在窗口加载完成后执行初始化
            await StartUp();
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
                LoggerHelper.LogInformation("安装游戏按钮被点击了，准备打开对话框。");

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
                        LoggerHelper.LogInformation($"从对话框中获取到的 JSON 数据: {selectedGameDataJson}");
                        try
                        {
                            // ⭐️ 反序列化 JSON 字符串为 GameModel 对象
                            GameModel? gameData = JsonSerializer.Deserialize<GameModel>(selectedGameDataJson);
                            if (gameData != null)
                            {
                                LoggerHelper.LogInformation($"解析到的游戏名称: {gameData.Name}, 路径: {gameData.Path}");
                                // ⭐️ 关键修改：调用 MainWindowViewModel 中的 HandleInstallGameAndUnzipAsync
                                await viewModel.HandleInstallGameAndUnzipAsync(gameData);
                            }
                            else
                            {
                                LoggerHelper.LogError("错误：无法将 JSON 数据反序列化为 GameModel 对象。");
                            }
                        }
                        catch (JsonException ex)
                        {
                            LoggerHelper.LogError($"JSON 反序列化错误: {ex.Message}");
                        }
                        catch (Exception ex)
                        {
                            LoggerHelper.LogError($"处理对话框结果时发生错误: {ex.Message}");
                        }
                    }
                }
                else
                {
                    LoggerHelper.LogInformation("用户取消了选择或对话框关闭。");
                }
            }
        }

        private void Setting_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                LoggerHelper.LogInformation("设置按钮被点击了！");
                viewModel.SettingCommand.Execute(null);
            }
        }


        private async Task StartUp()
        {
            try
            {
                if (!File.Exists(GlobalPaths.InitiatorProfileName))
                {
                    LoggerHelper.LogError("错误：配置文件不存在，请检查路径。");
                    PrimaryProfile();
                    return;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError($"错误：无法访问配置文件 - {ex.Message}");
                return;
            }
            LoggerHelper.LogInformation("配置文件存在，正在加载...");
            IniFile ini = new IniFile();
            ini.Load(GlobalPaths.InitiatorProfileName);
            var PrimaryProfileVersion = ini.GetSetting("main", "Version", string.Empty);
            if (PrimaryProfileVersion != GlobalPaths.Version)
            {
                LoggerHelper.LogWarning($"配置文件版本不匹配，当前版本：{GlobalPaths.Version}，配置文件版本：{PrimaryProfileVersion}");
                return;
            }
            GlobalPaths.TransitSoftwareLE = ini.GetSetting("main", "TransitSoftwareLE", string.Empty);
            GlobalPaths.GamePath = ini.GetSetting("main", "GamePath", string.Empty);

            await CheckForUpdatesAsync();

        }

        private async Task CheckForUpdatesAsync()
        {
            const string latestReleaseApi = "https://api.github.com/repos/Sjshi763/PicaComic/releases/latest";
            const string releasesPage = "https://github.com/Sjshi763/PicaComic/releases";

            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.UserAgent.ParseAdd("ATC4-HQ-UpdateChecker");

                var response = await client.GetAsync(latestReleaseApi);
                if (!response.IsSuccessStatusCode)
                {
                    LoggerHelper.LogWarning($"检查更新失败，状态码：{response.StatusCode}");
                    return;
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                var latestTag = doc.RootElement.GetProperty("tag_name").GetString() ?? string.Empty;
                var latestVersionText = latestTag.Trim().TrimStart('v', 'V');
                var currentVersionText = GlobalPaths.Version.Trim().TrimStart('v', 'V');

                if (!Version.TryParse(latestVersionText, out var latestVersion) ||
                    !Version.TryParse(currentVersionText, out var currentVersion))
                {
                    LoggerHelper.LogWarning($"版本号解析失败，当前版本：{GlobalPaths.Version}，远程版本：{latestTag}");
                    return;
                }

                if (latestVersion > currentVersion)
                {
                    ShowMessageDialog(this,
                        $"发现新版本\n当前版本：{GlobalPaths.Version}\n最新版本：{latestTag}\n请前往下载：{releasesPage}");
                }
                else
                {
                    LoggerHelper.LogInformation($"当前已是最新版本：{GlobalPaths.Version}");
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogWarning($"检查更新时发生异常：{ex.Message}");
            }
        }

        private void PrimaryProfile()
        {
            // 获取时间
            LoggerHelper.LogInformation("配置文件不存在，正在创建初始配置文件...");
            DateTimeOffset now = DateTimeOffset.UtcNow;
            LoggerHelper.LogDebug($"当前时间（UTC）：{now}");
            string generalShort = now.ToString("g");
            LoggerHelper.LogDebug($"当前时间：{generalShort}");

            // 获取加密时间
            if (!ATC4_HQ.ConfigProtector.TryProtect(generalShort, out string? encryptedText, GlobalPaths.Keys))
            {
                LoggerHelper.LogError("加密初始配置时间失败，已取消创建初始配置文件。");
                return;
            }

            LoggerHelper.LogDebug($"加密后的时间：{encryptedText}");

            //返回值
            GlobalPaths.FirstRun = encryptedText;

            LoggerHelper.LogInformation("创建初始配置文件...");
            IniFile ini = new IniFile();
            ini.SetSetting("main", "Version", GlobalPaths.Version);
            ini.SetSetting("main", "FirstRun", GlobalPaths.FirstRun ?? string.Empty);
            ini.SetSetting("main", "TransitSoftwareLE", "null");
            LoggerHelper.LogInformation("初始配置文件已创建。");
            ini.Save(GlobalPaths.InitiatorProfileName);
        }
        
        /// <summary>
        /// 处理DataContextChanged事件，订阅ShowOpenALInstallView事件
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        private void OnDataContextChanged(object? sender, EventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.ShowOpenALInstallView += OnShowOpenALInstallView;
            }
        }
        
        /// <summary>
        /// 处理显示OpenAL安装界面事件
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        private async void OnShowOpenALInstallView(object? sender, EventArgs e)
        {
            LoggerHelper.LogInformation("直接显示OpenAL安装界面");
            await ShowOpenALInstallView();
        }

        /// <summary>
        /// 显示OpenAL安装界面
        /// </summary>
        public async Task ShowOpenALInstallView()
        {
            // 显示OpenAL安装界面
            var openALInstallView = new OpenALInstallView();
            var openALInstallViewModel = new ViewModels.OpenALInstallViewModel();
            openALInstallView.DataContext = openALInstallViewModel;
            
            var openALInstallWindow = new Window
            {
                Title = "OPENAL 安装",
                Width = 500,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                CanResize = false,
                Content = openALInstallView
            };
            
            // 订阅ViewModel事件
            openALInstallViewModel.RequestClose += (s, e) => openALInstallWindow.Close();
            openALInstallViewModel.ShowMessage += (s, message) => ShowMessageDialog(openALInstallWindow, message);
            
            await openALInstallWindow.ShowDialog(this);
        }
        
        /// <summary>
        /// 显示消息对话框
        /// </summary>
        /// <param name="parentWindow">父窗口</param>
        /// <param name="message">消息内容</param>
        private async void ShowMessageDialog(Window parentWindow, string message)
        {
            var lines = message.Split('\n');
            string title = lines.Length > 0 ? lines[0] : "提示";
            string content = lines.Length > 1 ? string.Join("\n", lines[1..]) : message;
            
            // 创建测试结果对话框
            var resultWindow = new Window
            {
                Title = title,
                Width = 400,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                CanResize = false
            };

            var stackPanel = new StackPanel { Margin = new Thickness(20) };

            var titleTextBlock = new TextBlock
            {
                Text = title,
                FontSize = 20,
                FontWeight = Avalonia.Media.FontWeight.Bold,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            };

            var messageTextBlock = new TextBlock
            {
                Text = content,
                FontSize = 16,
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };

            var okButton = new Button
            {
                Content = "确定",
                Width = 80,
                Height = 35,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            };

            okButton.Click += (s, args) => resultWindow.Close();

            stackPanel.Children.Add(titleTextBlock);
            stackPanel.Children.Add(messageTextBlock);
            stackPanel.Children.Add(okButton);

            resultWindow.Content = stackPanel;
            
            // 显示对话框
            await resultWindow.ShowDialog(parentWindow);
        }
        
        /// <summary>
        /// 显示BT功能同意对话框
        /// </summary>
        
        /// <summary>
        /// 保存BT配置到配置文件
        /// </summary>
        /// <param name="enabled">是否启用BT</param>
    }
}
