using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using ATC4_HQ.Models; // 引入 GameModel 的命名空间
using SharpCompress.Archives;
using SharpCompress.Common;
using master.Globals;
using SoftCircuits.IniFileParser;
using System.IO; // 用于检查文件是否存在
using System.Collections.Generic; // 用于Stack
using System.Net.Http;
using System.Text.Json;
using System.IO.Compression;
using Avalonia.Media; // 引入 IBrush
using Avalonia.Controls; // 用于 Window 弹窗
using ATC4_HQ.Views; // 引入 ExtractProgressWindow

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
        private bool _isNavBtn4Checked;
        
        [ObservableProperty]
        private bool _canGoBack; // 是否可以返回

        /// <summary>
        /// Windows 个性化主色调 Brush，用于导航栏背景、ATC4 logo 文字等。
        /// </summary>
        [ObservableProperty]
        private IBrush _accentBrush = new SolidColorBrush(Color.Parse(GlobalPaths.AccentColor.main));

        /// <summary>
        /// 主色调的浅色变体 Brush，用于辅助标签（如 HQ 标签）背景。
        /// </summary>
        [ObservableProperty]
        private IBrush _accentLightBrush = new SolidColorBrush(Color.Parse(GlobalPaths.AccentColor.Light));
        
        // 导航历史记录
        private Stack<ViewModelBase> _navigationHistory = new Stack<ViewModelBase>();
        
        // 事件：当需要显示OPENAL未安装警告时触发
        public event EventHandler? OpenALNotInstalled;
        
        // 事件：当需要显示OPENAL安装界面时触发
        public event EventHandler? ShowOpenALInstallView;
        
        // 事件：当检测到有新版本时触发（由 View 层决定是否弹窗与后续操作）
        public event EventHandler<UpdateAvailableEventArgs>? UpdateAvailable;
        
        // 事件：当需要显示进度窗口时触发，参数为 ExtractProgressViewModel
        public event EventHandler<ShowProgressWindowEventArgs>? ShowProgressWindowRequested;
        
        // 事件：当需要关闭进度窗口时触发
        public event EventHandler? CloseProgressWindowRequested;
        
        public ICommand StartGameCommand { get; }
        public ICommand InstallGameCommand { get; } // 用于 ViewModel 内部逻辑或未来绑定
        public ICommand SettingCommand { get; }
        public ICommand NavigateCommand { get; } // 新增导航命令
        public ICommand GoBackCommand { get; } // 新增返回命令

        public MainWindowViewModel()
        {
            StartGameCommand = new RelayCommand(OnStartGame);
            InstallGameCommand = new RelayCommand(OnInstallGame); 
            SettingCommand = new RelayCommand(OnSetting);
            NavigateCommand = new RelayCommand<string>(OnNavigate);
            GoBackCommand = new RelayCommand(OnGoBack, () => CanGoBack);
            
            // 加载游戏列表
            LoadGamesList();
            
            // 初始化默认页面
            OnStartGame();
        }

        public async Task CheckForUpdatesAsync()
        {
            const string latestReleaseApi = "https://api.github.com/repos/Sjshi763/ATC4-HQ/releases/latest";
            const string releasesPage = "https://github.com/Sjshi763/ATC4-HQ/releases";

            try
            {
                LoggerHelper.LogInformation($"开始检查更新，当前版本：{GlobalPaths.Version}");
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
                LoggerHelper.LogInformation($"更新检查返回版本信息，当前：{currentVersionText}，远程：{latestVersionText}");

                if (!Version.TryParse(latestVersionText, out var latestVersion) ||
                    !Version.TryParse(currentVersionText, out var currentVersion))
                {
                    LoggerHelper.LogWarning($"版本号解析失败，当前版本：{GlobalPaths.Version}，远程版本：{latestTag}");
                    return;
                }

                if (latestVersion > currentVersion)
                {
                    LoggerHelper.LogInformation($"检测到新版本，当前：{GlobalPaths.Version}，最新：{latestTag}，准备通知界面层。");
                    UpdateAvailable?.Invoke(this, new UpdateAvailableEventArgs(GlobalPaths.Version, latestTag, releasesPage));
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
            LoggerHelper.LogInformation("ViewModel: 显示安装数据界面。");
            CurrentSubPage = new InstallGameDataViewModel();
            // 更新导航按钮状态
            IsNavBtn1Checked = false;
            IsNavBtn2Checked = true;
            IsNavBtn4Checked = false;
            LoggerHelper.LogInformation("ViewModel: 已在右边显示安装数据界面。");
        }

        private void OnSetting()
        {
            LoggerHelper.LogInformation("启动设置");
            CurrentSubPage = new SettingViewModel();
            // 更新导航按钮状态
            IsNavBtn1Checked = false;
            IsNavBtn2Checked = false;
            IsNavBtn4Checked = true;
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
            IsNavBtn2Checked = page is InstallGameDataViewModel;
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
            if (string.IsNullOrWhiteSpace(gameData.ArchivePath) || !Directory.Exists(gameData.ArchivePath))
            {
                LoggerHelper.LogError($"错误：压缩包文件夹不存在：{gameData.ArchivePath}");
                return;
            }

            if (string.IsNullOrWhiteSpace(gameData.Path))
            {
                LoggerHelper.LogError("错误：安装路径为空");
                return;
            }

            var missingParts = GetMissingArchiveParts(gameData.ArchivePath);
            if (missingParts.Count > 0)
            {
                LoggerHelper.LogError($"错误：压缩包文件夹不完整，缺少：{string.Join("、", missingParts)}");
                return;
            }

            var zipPath = Path.Combine(gameData.ArchivePath, $"{GlobalPaths.Atc4ArchiveBaseName}.zip");
            LoggerHelper.LogInformation($"开始分卷解压文件：{zipPath} 到目录：{gameData.Path}");

            var progressViewModel = new ExtractProgressViewModel();
            ShowProgressWindowRequested?.Invoke(this, new ShowProgressWindowEventArgs(progressViewModel, gameData));

            var nestedArchiveEntries = new List<string>();
            var nestedArchiveFileCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            int mainArchiveFileCount = 0;

            static int CountFilesInArchive(IArchive archive)
            {
                int count = 0;
                foreach (var archiveEntry in archive.Entries)
                {
                    if (!archiveEntry.IsDirectory)
                    {
                        count++;
                    }
                }

                return count;
            }

            static bool IsNestedZipEntry(string? entryKey)
            {
                if (string.IsNullOrWhiteSpace(entryKey))
                {
                    return false;
                }

                var fileName = Path.GetFileName(entryKey);
                return fileName.StartsWith("~", StringComparison.Ordinal) &&
                       fileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase);
            }

            static string GetSafeDestinationPath(string rootPath, string? entryKey)
            {
                string relativePath = entryKey?.TrimStart('~') ?? string.Empty;
                if (string.IsNullOrWhiteSpace(relativePath))
                {
                    throw new InvalidOperationException("压缩包条目路径为空。");
                }

                string destinationPath = Path.GetFullPath(Path.Combine(rootPath, relativePath));
                string destinationRoot = Path.GetFullPath(rootPath)
                    .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    + Path.DirectorySeparatorChar;

                if (!destinationPath.StartsWith(destinationRoot, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException($"检测到不安全的压缩包条目：{entryKey}");
                }

                return destinationPath;
            }

            void EnsureNotCancelled()
            {
                if (progressViewModel.IsCancelled)
                {
                    throw new OperationCanceledException("用户取消了解压操作");
                }
            }

            try
            {
                progressViewModel.AddLog("开始扫描压缩包步骤...");
                progressViewModel.UpdateStepProgress(0, 1, "正在扫描安装步骤...");

                using (var archiveForScanning = ArchiveFactory.OpenArchive(zipPath))
                {
                    foreach (var entry in archiveForScanning.Entries)
                    {
                        if (entry.IsDirectory)
                        {
                            continue;
                        }

                        mainArchiveFileCount++;

                        if (!IsNestedZipEntry(entry.Key))
                        {
                            continue;
                        }

                        using var entryStream = entry.OpenEntryStream();
                        using var memoryStream = new MemoryStream();
                        await entryStream.CopyToAsync(memoryStream);
                        memoryStream.Position = 0;

                        using var nestedZipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Read, leaveOpen: false);
                        int nestedFileCount = 0;
                        foreach (var nestedEntry in nestedZipArchive.Entries)
                        {
                            if (!string.IsNullOrWhiteSpace(nestedEntry.Name))
                            {
                                nestedFileCount++;
                            }
                        }

                        nestedArchiveEntries.Add(entry.Key ?? string.Empty);
                        nestedArchiveFileCounts[entry.Key ?? string.Empty] = nestedFileCount;
                    }
                }

                int nestedArchiveFileStepCount = 0;
                foreach (var nestedFileCount in nestedArchiveFileCounts.Values)
                {
                    nestedArchiveFileStepCount += nestedFileCount;
                }

                int totalSteps =
                    1 + // 创建安装目录
                    1 + // 扫描主压缩包
                    mainArchiveFileCount +
                    1 + // 扫描子压缩包
                    nestedArchiveEntries.Count +
                    nestedArchiveFileStepCount +
                    1 + // 写入 GameData.ini
                    1 + // 更新游戏列表
                    1 + // 保存配置
                    1;  // 完成安装

                int completedSteps = 0;

                void AdvanceStep(string message, string? logMessage = null)
                {
                    completedSteps++;
                    progressViewModel.UpdateStepProgress(completedSteps, totalSteps, message);
                    if (!string.IsNullOrWhiteSpace(logMessage))
                    {
                        progressViewModel.AddLog(logMessage);
                    }
                }

                void MarkCurrentState(string message, string? logMessage = null)
                {
                    progressViewModel.UpdateStepProgress(completedSteps, totalSteps, message);
                    if (!string.IsNullOrWhiteSpace(logMessage))
                    {
                        progressViewModel.AddLog(logMessage);
                    }
                }

                EnsureNotCancelled();
                Directory.CreateDirectory(gameData.Path);
                AdvanceStep("正在创建安装目录...", $"已创建安装目录：{gameData.Path}");

                EnsureNotCancelled();
                AdvanceStep("主压缩包扫描完成", $"主压缩包包含 {mainArchiveFileCount} 个文件");

                EnsureNotCancelled();
                AdvanceStep("子压缩包扫描完成", $"发现 {nestedArchiveEntries.Count} 个待二次解压的 ZIP 文件");

                using (var archive = ArchiveFactory.OpenArchive(zipPath))
                {
                    foreach (var entry in archive.Entries)
                    {
                        if (entry.IsDirectory)
                        {
                            continue;
                        }

                        EnsureNotCancelled();

                        using var entryStream = entry.OpenEntryStream();
                        using var memoryStream = new MemoryStream();
                        await entryStream.CopyToAsync(memoryStream);
                        byte[] entryData = memoryStream.ToArray();

                        string destinationPath = GetSafeDestinationPath(gameData.Path, entry.Key);
                        string? destinationParent = Path.GetDirectoryName(destinationPath);
                        if (!string.IsNullOrWhiteSpace(destinationParent))
                        {
                            Directory.CreateDirectory(destinationParent);
                        }

                        await File.WriteAllBytesAsync(destinationPath, entryData);
                        string fileName = Path.GetFileName(destinationPath);

                        AdvanceStep($"正在处理主包文件：{fileName}", $"已写入主包文件：{fileName}");
                    }
                }

                var extractedZipFiles = Directory.GetFiles(gameData.Path, "*.zip", SearchOption.AllDirectories);
                foreach (var file in extractedZipFiles)
                {
                    var fileName = Path.GetFileName(file);
                    if (!fileName.StartsWith("~", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    EnsureNotCancelled();
                    AdvanceStep($"正在解压子压缩包：{fileName}", $"开始处理子压缩包：{fileName}");

                    using var subArchive = ArchiveFactory.OpenArchive(file);
                    foreach (var entry in subArchive.Entries)
                    {
                        if (entry.IsDirectory)
                        {
                            continue;
                        }

                        EnsureNotCancelled();

                        using var entryStream = entry.OpenEntryStream();
                        using var memoryStream = new MemoryStream();
                        await entryStream.CopyToAsync(memoryStream);
                        byte[] entryData = memoryStream.ToArray();

                        string destinationPath = GetSafeDestinationPath(gameData.Path, entry.Key);
                        string? destinationParent = Path.GetDirectoryName(destinationPath);
                        if (!string.IsNullOrWhiteSpace(destinationParent))
                        {
                            Directory.CreateDirectory(destinationParent);
                        }

                        await File.WriteAllBytesAsync(destinationPath, entryData);
                        string nestedFileName = Path.GetFileName(destinationPath);

                        AdvanceStep($"正在处理子包文件：{nestedFileName}", $"已写入子包文件：{nestedFileName}");
                    }

                    File.Delete(file);
                    MarkCurrentState($"已完成子压缩包：{fileName}", $"已删除临时子压缩包：{fileName}");
                }

                EnsureNotCancelled();
                var gameDataIniPath = Path.Combine(gameData.Path, "GameData.ini");
                var ini = new IniFile();
                if (File.Exists(gameDataIniPath))
                {
                    ini.Load(gameDataIniPath);
                }

                ini.SetSetting("GameSettings", "GameName", gameData.Name);
                ini.Save(gameDataIniPath);
                AdvanceStep("正在写入游戏配置...", "已写入 GameData.ini");

                EnsureNotCancelled();
                GlobalPaths.Games.Add(gameData);
                GlobalPaths.CurrentGame = gameData;
                AdvanceStep("正在更新游戏列表...", $"已添加游戏到内存列表：{gameData.Name}");

                EnsureNotCancelled();
                SaveGamesList();
                AdvanceStep("正在保存启动器配置...", "已保存游戏列表配置");

                LoggerHelper.LogInformation($"游戏安装成功：{gameData.Name} -> {gameData.Path} 喵");
                AdvanceStep("安装完成，正在收尾...", "解压与安装步骤全部完成");

                progressViewModel.OnExtractionCompleted();
                CloseProgressWindowRequested?.Invoke(this, EventArgs.Empty);
                ClearSubPage();
            }
            catch (OperationCanceledException)
            {
                progressViewModel.AddLog("用户已取消解压，正在清理...");
                CleanupExtractedFiles(gameData.Path);
                progressViewModel.StatusMessage = "已取消";
                progressViewModel.StepDetail = "安装已取消";
            }
            catch (Exception ex)
            {
                progressViewModel.OnExtractionFailed(ex.Message);
                LoggerHelper.LogError($"解压失败：{ex.Message}");
            }
        }

        private static List<string> GetMissingArchiveParts(string folderPath)
        {
            var missingParts = new List<string>();
            foreach (var archivePart in GlobalPaths.RequiredAtc4ArchiveParts)
            {
                if (!File.Exists(Path.Combine(folderPath, archivePart)))
                {
                    missingParts.Add(archivePart);
                }
            }

            return missingParts;
        }

        private static void ExtractArchiveToDirectory(string archivePath, string destinationDirectory, bool trimLeadingTilde = false)
        {
            Directory.CreateDirectory(destinationDirectory);

            using var archive = ArchiveFactory.OpenArchive(archivePath);
            foreach (var entry in archive.Entries)
            {
                if (entry.IsDirectory)
                {
                    continue;
                }

                if (!trimLeadingTilde)
                {
                    entry.WriteToDirectory(destinationDirectory, new ExtractionOptions
                    {
                        ExtractFullPath = true,
                        Overwrite = true
                    });
                    continue;
                }

                var normalizedEntryPath = entry.Key?.TrimStart('~');
                if (string.IsNullOrWhiteSpace(normalizedEntryPath))
                {
                    continue;
                }

                var destinationPath = Path.GetFullPath(Path.Combine(destinationDirectory, normalizedEntryPath));
                var destinationRoot = Path.GetFullPath(destinationDirectory)
                    .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    + Path.DirectorySeparatorChar;

                if (!destinationPath.StartsWith(destinationRoot, StringComparison.OrdinalIgnoreCase))
                {
                    LoggerHelper.LogWarning($"跳过不安全的压缩包条目：{entry.Key}");
                    continue;
                }

                var destinationParent = Path.GetDirectoryName(destinationPath);
                if (!string.IsNullOrWhiteSpace(destinationParent))
                {
                    Directory.CreateDirectory(destinationParent);
                }

                using var entryStream = entry.OpenEntryStream();
                using var destinationStream = File.Create(destinationPath);
                entryStream.CopyTo(destinationStream);
            }
        }

        /// <summary>
        /// 清理解压失败或取消后残留的文件
        /// </summary>
        private static void CleanupExtractedFiles(string directory)
        {
            try
            {
                if (Directory.Exists(directory))
                {
                    LoggerHelper.LogInformation($"清理目录：{directory}");
                    Directory.Delete(directory, recursive: true);
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError($"清理文件失败：{ex.Message}");
            }
        }

        private void AddLog(string message)
        {
            LoggerHelper.LogInformation(message);
        }
        
        /// <summary>
        /// 清除右边区域的内容
        /// </summary>
        public void ClearSubPage()
        {
            CurrentSubPage = null;
            LoggerHelper.LogInformation("已清除右边区域的内容。");
        }

        /// <summary>
        /// 从配置文件加载游戏列表
        /// </summary>
        private void LoadGamesList()
        {
            GlobalPaths.Games.Clear();
            
            var ini = new IniFile();
            if (File.Exists(GlobalPaths.InitiatorProfileName))
            {
                try
                {
                    ini.Load(GlobalPaths.InitiatorProfileName);
                    
                    // 读取游戏数量
                    int gameCount = 0;
                    if (int.TryParse(ini.GetSetting("Games", "Count", "0"), out int count))
                    {
                        gameCount = count;
                    }
                    
                    // 读取每个游戏
                    for (int i = 0; i < gameCount; i++)
                    {
                        string section = $"Game{i}";
                        string name = ini.GetSetting(section, "Name", string.Empty);
                        string path = ini.GetSetting(section, "Path", string.Empty);
                        string archivePath = ini.GetSetting(section, "ArchivePath", string.Empty);
                        
                        if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(path))
                        {
                            GlobalPaths.Games.Add(new GameModel
                            {
                                Name = name,
                                Path = path,
                                ArchivePath = archivePath
                            });
                        }
                    }
                    
                    // 设置当前游戏为第一个
                    if (GlobalPaths.Games.Count > 0)
                    {
                        GlobalPaths.CurrentGame = GlobalPaths.Games[0];
                    }
                    
                    LoggerHelper.LogInformation($"已加载 {GlobalPaths.Games.Count} 个游戏配置 喵");
                }
                catch (Exception ex)
                {
                    LoggerHelper.LogError($"加载游戏列表失败：{ex.Message} 喵");
                }
            }
            else
            {
                LoggerHelper.LogInformation("配置文件不存在，游戏列表为空 喵");
            }
        }

        /// <summary>
        /// 保存游戏列表到配置文件
        /// </summary>
        private void SaveGamesList()
        {
            try
            {
                var ini = new IniFile();
                if (File.Exists(GlobalPaths.InitiatorProfileName))
                {
                    ini.Load(GlobalPaths.InitiatorProfileName);
                }
                
                // 保存游戏数量
                ini.SetSetting("Games", "Count", GlobalPaths.Games.Count.ToString());
                
                // 保存每个游戏
                for (int i = 0; i < GlobalPaths.Games.Count; i++)
                {
                    string section = $"Game{i}";
                    var game = GlobalPaths.Games[i];
                    ini.SetSetting(section, "Name", game.Name);
                    ini.SetSetting(section, "Path", game.Path);
                    ini.SetSetting(section, "ArchivePath", game.ArchivePath);
                }
                
                ini.Save(GlobalPaths.InitiatorProfileName);
                LoggerHelper.LogInformation($"已保存 {GlobalPaths.Games.Count} 个游戏配置 喵");
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError($"保存游戏列表失败：{ex.Message} 喵");
            }
        }
    }

    public sealed class UpdateAvailableEventArgs : EventArgs
    {
        public string CurrentVersion { get; }
        public string LatestVersion { get; }
        public string ReleasesPageUrl { get; }

        public UpdateAvailableEventArgs(string currentVersion, string latestVersion, string releasesPageUrl)
        {
            CurrentVersion = currentVersion;
            LatestVersion = latestVersion;
            ReleasesPageUrl = releasesPageUrl;
        }
    }

    /// <summary>
    /// 显示进度窗口事件参数
    /// </summary>
    public sealed class ShowProgressWindowEventArgs : EventArgs
    {
        public ExtractProgressViewModel ProgressViewModel { get; }
        public GameModel GameData { get; }

        public ShowProgressWindowEventArgs(ExtractProgressViewModel progressViewModel, GameModel gameData)
        {
            ProgressViewModel = progressViewModel;
            GameData = gameData;
        }
    }
}
