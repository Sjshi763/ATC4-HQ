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
using System.Threading.Tasks; // 添加Task支持

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
            Console.WriteLine("直接显示OpenAL安装界面");
            await ShowOpenALInstallView();
        }

        /// <summary>
        /// 显示OpenAL安装界面
        /// </summary>
        public async Task ShowOpenALInstallView()
        {
            // 显示OpenAL安装界面
            var openALInstallView = new OpenALInstallView();
            var openALInstallWindow = new Window
            {
                Title = "OPENAL 安装",
                Width = 500,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                CanResize = false,
                Content = openALInstallView
            };
            await openALInstallWindow.ShowDialog(this);
        }
    }
}
