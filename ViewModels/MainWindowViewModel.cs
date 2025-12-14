using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using ATC4_HQ.Models; // 引入 GameModel 的命名空间
using System.IO.Compression; // 用于解压缩功能
using master.Globals;
using Masuit.Tools.Files;
using System.IO; // 用于检查文件是否存在
using System.Collections.Generic; // 用于Stack

namespace ATC4_HQ.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ViewModelBase? _currentPage; // 当前显示在 PageHost 中的 ViewModel
        
        [ObservableProperty]
        private ViewModelBase? _currentSubPage; // 右侧内容区域的ViewModel
        
        [ObservableProperty]
        private bool _isNavBtn1Checked = true; // 默认选中第一个导航按钮
        
        [ObservableProperty]
        private bool _isNavBtn2Checked;
        
        [ObservableProperty]
        private bool _isNavBtn3Checked;
        
        [ObservableProperty]
        private bool _isNavBtn4Checked;
        
        [ObservableProperty]
        private bool _canGoBack; // 是否可以返回
        
        // 导航历史记录
        private Stack<ViewModelBase> _navigationHistory = new Stack<ViewModelBase>();
        
        // 事件：当需要显示OPENAL未安装警告时触发
        public event EventHandler? OpenALNotInstalled;
        
        // 事件：当需要显示OPENAL安装界面时触发
        public event EventHandler? ShowOpenALInstallView;
        
        public ICommand StartGameCommand { get; }
        public ICommand InstallGameCommand { get; } // 用于 ViewModel 内部逻辑或未来绑定
        public ICommand SettingCommand { get; }
        public ICommand DownloadMonitorCommand { get; }
        public ICommand NavigateCommand { get; } // 新增导航命令
        public ICommand GoBackCommand { get; } // 新增返回命令

        public MainWindowViewModel()
        {
            StartGameCommand = new RelayCommand(OnStartGame);
            InstallGameCommand = new RelayCommand(OnInstallGame); 
            SettingCommand = new RelayCommand(OnSetting);
            DownloadMonitorCommand = new RelayCommand(OnDownloadMonitor);
            NavigateCommand = new RelayCommand<string>(OnNavigate);
            GoBackCommand = new RelayCommand(OnGoBack, () => CanGoBack);
            
            // 初始化默认页面
            OnStartGame();
        }

        private void OnStartGame()
        {
            LoggerHelper.LogInformation("ViewModel: 启动游戏逻辑。");
            
            // 检查OPENAL是否安装
            if (!IsOpenALInstalled())
            {
                LoggerHelper.LogWarning("ViewModel: OPENAL未安装，触发显示安装界面事件。");
                // 触发事件通知View显示OpenAL安装界面
                ShowOpenALInstallView?.Invoke(this, EventArgs.Empty);
                return; // 不继续执行后续逻辑
            }
            
            // 实现启动游戏的业务逻辑
            // 将 CurrentPage 设置为 GameStartOptionsViewModel 的实例
            NavigateToPage(new GameStartOptionsViewModel(this), 1);
            LoggerHelper.LogInformation("ViewModel: 已切换到游戏启动选项界面。");
        }

        private void OnInstallGame()
        {
            LoggerHelper.LogInformation("ViewModel: 显示安装游戏界面。");
            // 将 CurrentSubPage 设置为 InstallGameViewModel 的实例，显示在右边
            CurrentSubPage = new InstallGameViewModel();
            // 更新导航按钮状态
            IsNavBtn1Checked = false;
            IsNavBtn2Checked = true;
            IsNavBtn3Checked = false;
            IsNavBtn4Checked = false;
            LoggerHelper.LogInformation("ViewModel: 已在右边显示安装游戏界面。");
        }

        private void OnSetting()
        {
            LoggerHelper.LogInformation("启动设置");
            CurrentSubPage = new SettingViewModel();
            // 更新导航按钮状态
            IsNavBtn1Checked = false;
            IsNavBtn2Checked = false;
            IsNavBtn3Checked = false;
            IsNavBtn4Checked = true;
        }

        private void OnDownloadMonitor()
        {
            LoggerHelper.LogInformation("打开下载监视器");
            NavigateToPage(new DownloadMonitorViewModel(), 3);
        }
        
        /// <summary>
        /// 导航到指定页面
        /// </summary>
        /// <param name="page">要导航到的页面</param>
        /// <param name="navButtonIndex">导航按钮索引</param>
        private void NavigateToPage(ViewModelBase page, int navButtonIndex)
        {
            // 保存当前页面到历史记录
            if (CurrentPage != null)
            {
                _navigationHistory.Push(CurrentPage);
            }
            
            CurrentPage = page;
            CurrentSubPage = null; // 清除子页面
            
            // 更新导航按钮状态
            IsNavBtn1Checked = navButtonIndex == 1;
            IsNavBtn2Checked = navButtonIndex == 2;
            IsNavBtn3Checked = navButtonIndex == 3;
            IsNavBtn4Checked = navButtonIndex == 4;
            
            // 更新返回按钮状态
            CanGoBack = _navigationHistory.Count > 0;
        }
        
        /// <summary>
        /// 处理导航命令
        /// </summary>
        /// <param name="parameter">导航参数</param>
        private void OnNavigate(string? parameter)
        {
            if (parameter == null) return;
            
            switch (parameter)
            {
                case "1":
                    OnStartGame();
                    break;
                case "2":
                    // 安装游戏逻辑 - 显示安装游戏界面
                    OnInstallGame();
                    break;
                case "3":
                    OnDownloadMonitor();
                    break;
                case "4":
                    OnSetting();
                    break;
            }
        }
        
        /// <summary>
        /// 处理返回命令
        /// </summary>
        private void OnGoBack()
        {
            if (_navigationHistory.Count > 0)
            {
                var previousPage = _navigationHistory.Pop();
                CurrentPage = previousPage;
                CurrentSubPage = null;
                
                // 更新返回按钮状态
                CanGoBack = _navigationHistory.Count > 0;
                
                // 更新导航按钮状态（根据页面类型）
                UpdateNavButtonState(previousPage);
            }
        }
        
        /// <summary>
        /// 根据页面类型更新导航按钮状态
        /// </summary>
        /// <param name="page">当前页面</param>
        private void UpdateNavButtonState(ViewModelBase page)
        {
            IsNavBtn1Checked = page is GameStartOptionsViewModel;
            IsNavBtn2Checked = page is InstallGameViewModel;
            IsNavBtn3Checked = page is DownloadMonitorViewModel;
            IsNavBtn4Checked = page is SettingViewModel;
        }

        /// <summary>
        /// 检查OPENAL是否安装
        /// </summary>
        /// <returns>如果OPENAL已安装返回true，否则返回false</returns>
        private bool IsOpenALInstalled()
        {
            // 检查系统目录中是否存在openal32.dll
            string openalPath = Path.Combine(Environment.SystemDirectory, "openal32.dll");
            return File.Exists(openalPath);
        }

        // 处理游戏安装和解压的通用方法，现在接收 GameModel 对象
        public async Task HandleInstallGameAndUnzipAsync(GameModel gameData) // ⭐️ 确保方法是 public 且接收 GameModel
        {
            // 检查游戏路径是否包含zip文件
            string zipPath = string.Empty;
            if (gameData.Path.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                zipPath = gameData.Path;
                // 如果路径是zip文件，则使用其所在目录作为解压目标
                gameData.Path = Path.GetDirectoryName(gameData.Path) ?? gameData.Path;
            }
            else
            {
                // 查找路径下的zip文件
                var zipFiles = Directory.GetFiles(gameData.Path, "*.zip");
                if (zipFiles.Length > 0)
                {
                    zipPath = zipFiles[0];
                }
                else
                {
                    LoggerHelper.LogError("错误：在指定路径中未找到zip文件。");
                    return;
                }
            }

            if (!File.Exists(zipPath))
            {
                LoggerHelper.LogError($"错误：zip文件不存在：{zipPath}");
                return;
            }

            LoggerHelper.LogInformation($"开始解压文件：{zipPath} 到目录：{gameData.Path}");
            
            // 解压 .zip 文件到指定目录
            ZipFile.ExtractToDirectory(zipPath, gameData.Path);
            LoggerHelper.LogInformation("文件解压完成。");

            // 检查解压后的文件中是否有以~开头的zip文件，并再次解压
            var extractedFiles = Directory.GetFiles(gameData.Path, "*.zip", SearchOption.AllDirectories);
            foreach (var file in extractedFiles)
            {
                var fileName = Path.GetFileName(file);
                if (fileName.StartsWith("~"))
                {
                    // 使用一个新的 MemoryStream 来读取和解压，避免文件占用问题
                    using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await fileStream.CopyToAsync(memoryStream);
                            memoryStream.Position = 0; // 重置流的位置

                            using (ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Read))
                            {
                                foreach (ZipArchiveEntry entry in archive.Entries)
                                {
                                    // 去掉条目前面的波浪号，构造正确的目标路径
                                    string destinationPath = Path.Combine(gameData.Path, entry.FullName.TrimStart('~'));
                                    
                                    // 确保目标目录存在
                                    string? destinationDirectory = Path.GetDirectoryName(destinationPath);
                                    if (destinationDirectory != null)
                                    {
                                        Directory.CreateDirectory(destinationDirectory);
                                    }

                                    // 如果不是目录，则解压文件
                                    if (!string.IsNullOrEmpty(entry.Name))
                                    {
                                        entry.ExtractToFile(destinationPath, true);
                                    }
                                }
                            }
                        }
                    }
                    File.Delete(file); // 解压完成后删除这个临时的zip文件
                }
            }

            GlobalPaths.GamePath = gameData.Path; // 更新全局路径
            IniFile ini = new IniFile(GlobalPaths.InitiatorProfileName);
            ini.SetValue("main", "GamePath", GlobalPaths.GamePath);
            ini.Save();
            GlobalPaths.GameName = gameData.Name;
            ini = new IniFile(GlobalPaths.GamePath + @"\GameData.ini");
            ini.SetValue("GameSettings" , "GameName" , GlobalPaths.GameName);
            ini.Save();
            
            // 安装完成后清除右边区域的内容
            ClearSubPage();
        }
        
        /// <summary>
        /// 清除右边区域的内容
        /// </summary>
        public void ClearSubPage()
        {
            CurrentSubPage = null;
            LoggerHelper.LogInformation("已清除右边区域的内容。");
        }
    }
}
