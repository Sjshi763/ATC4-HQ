using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using ATC4_HQ.Models;
using System.IO;
using System.Text.Json;
using System.Net.Http;
using master.Globals;
using ATC4_HQ.ViewModels;
using System.IO.Compression;

namespace ATC4_HQ.ViewModels
{
    public partial class DownloadMonitorViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ObservableCollection<DownloadItemViewModel> _downloadItems = new();
        
        [ObservableProperty]
        private DownloadItemViewModel? _selectedDownloadItem;
        
        [ObservableProperty]
        private bool _isAllPaused;
        
        [ObservableProperty]
        private string _totalSpeed = "0 B/s";
        
        [ObservableProperty]
        private double _totalProgress;
        
        private readonly Timer _updateTimer;
        private readonly string _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "download-config.json");
        private readonly BtService _btService = new BtService();
        private readonly HttpClient _httpClient = new HttpClient();
        private const string GitHubJsonUrl = "https://github.com/ATC4-HQ-DATA/install-files/files-link.json"; // TODO: 请替换为你的实际GitHub仓库URL

        public ICommand StartAllCommand { get; }
        public ICommand PauseAllCommand { get; }
        public ICommand RemoveCompletedCommand { get; }
        public ICommand AddDownloadCommand { get; }
        public ICommand StartSelectedCommand { get; }
        public ICommand PauseSelectedCommand { get; }
        public ICommand RemoveSelectedCommand { get; }

        public DownloadMonitorViewModel()
        {
            StartAllCommand = new RelayCommand(StartAllDownloads);
            PauseAllCommand = new RelayCommand(PauseAllDownloads);
            RemoveCompletedCommand = new RelayCommand(RemoveCompletedDownloads);
            AddDownloadCommand = new RelayCommand(async () => await AddDownload());
            StartSelectedCommand = new RelayCommand(StartSelectedDownload, () => SelectedDownloadItem != null);
            PauseSelectedCommand = new RelayCommand(PauseSelectedDownload, () => SelectedDownloadItem != null);
            RemoveSelectedCommand = new RelayCommand(RemoveSelectedDownload, () => SelectedDownloadItem != null);

            _updateTimer = new Timer(1000); // 每秒更新一次
            _updateTimer.Elapsed += UpdateTimer_Elapsed;
            _updateTimer.Start();

            // 初始化BT服务
            InitializeBtService();
            
            LoadDownloadsFromConfig();
            _ = LoadDefaultDownloadsFromGitHub(); // 异步加载
        }

        private void InitializeBtService()
        {
            var downloadPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Downloads");
            Directory.CreateDirectory(downloadPath);
            
            _btService.InitAsync(downloadPath);
            
            // 设置事件处理器
            _btService.DownloadProgressChanged += OnDownloadProgressChanged;
            _btService.DownloadCompleted += OnDownloadCompleted;
            
            // 设置BtService到DownloadItemViewModel
            DownloadItemViewModel.SetBtService(_btService);
        }

        private void OnDownloadProgressChanged(object? sender, DownloadProgressEventArgs e)
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                var item = DownloadItems.FirstOrDefault(x => x.Name == e.GameName);
                if (item != null)
                {
                    item.UpdateProgress(e.Progress, e.DownloadedBytes, e.TotalBytes, e.DownloadSpeed);
                }
            });
        }

        private void OnDownloadCompleted(object? sender, DownloadCompletedEventArgs e)
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                var item = DownloadItems.FirstOrDefault(x => x.Name == e.GameName);
                if (item != null)
                {
                    item.UpdateProgress(100, item.TotalBytes, item.TotalBytes, 0);
                    item.Status = "下载完成，正在解压...";
                    
                    // 开始解压
                    _ = Task.Run(() => ExtractDownloadedGame(e.GameName, e.DownloadPath));
                }
            });
        }

        private async Task ExtractDownloadedGame(string gameName, string downloadPath)
        {
            try
            {
                // 查找下载目录中的zip文件
                var zipFiles = Directory.GetFiles(downloadPath, "*.zip", SearchOption.AllDirectories);
                if (zipFiles.Length > 0)
                {
                    var zipFile = zipFiles[0];
                    var extractPath = Path.Combine(downloadPath, "extracted");
                    Directory.CreateDirectory(extractPath);
                    
                    // 解压文件
                    ZipFile.ExtractToDirectory(zipFile, extractPath);
                    
                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        var item = DownloadItems.FirstOrDefault(x => x.Name == gameName);
                        if (item != null)
                        {
                            item.Status = "解压完成";
                        }
                        
                        LoggerHelper.LogInformation($"游戏 {gameName} 解压完成: {extractPath}");
                    });
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError($"解压游戏 {gameName} 失败: {ex.Message}");
                
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    var item = DownloadItems.FirstOrDefault(x => x.Name == gameName);
                    if (item != null)
                    {
                        item.Status = "解压失败";
                        item.ErrorMessage = ex.Message;
                    }
                });
            }
        }

        private async Task LoadDefaultDownloadsFromGitHub()
        {
            try
            {
                LoggerHelper.LogInformation("正在从GitHub获取游戏列表...");
                
                // 尝试从GitHub获取JSON文件
                var response = await _httpClient.GetAsync(GitHubJsonUrl);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<JsonElement>(json);
                    
                    if (data.TryGetProperty("main-games", out var games))
                    {
                        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                        {
                            foreach (var game in games.EnumerateArray())
                            {
                                string name = game.GetProperty("name").GetString() ?? "未知游戏";
                                string url = game.GetProperty("url").GetString() ?? "";
                                
                                // 检查是否已经存在
                                if (!DownloadItems.Any(item => item.MagnetLink == url))
                                {
                                    var newItem = new DownloadItemViewModel(name, url);
                                    DownloadItems.Add(newItem);
                                    LoggerHelper.LogInformation($"添加游戏到下载列表: {name}");
                                }
                            }
                            
                            // 自动开始下载
                            if (GlobalPaths.BTEnabled)
                            {
                                LoggerHelper.LogInformation("自动开始下载游戏...");
                                StartAllDownloads();
                            }
                        });
                        
                        LoggerHelper.LogInformation("从GitHub获取游戏列表成功");
                    }
                }
                else
                {
                    LoggerHelper.LogError($"从GitHub获取游戏列表失败: {response.StatusCode}");
                    // 如果GitHub获取失败，使用本地文件
                    LoadDefaultDownloads();
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError($"从GitHub获取游戏列表失败: {ex.Message}");
                // 如果GitHub获取失败，使用本地文件
                LoadDefaultDownloads();
            }
        }

        private void LoadDefaultDownloads()
        {
            try
            {
                string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ATC4-HQ-DATA", "install-files", "files-link.json");
                if (File.Exists(jsonPath))
                {
                    var json = File.ReadAllText(jsonPath);
                    var data = JsonSerializer.Deserialize<JsonElement>(json);
                    
                    if (data.TryGetProperty("main-games", out var games))
                    {
                        foreach (var game in games.EnumerateArray())
                        {
                            string name = game.GetProperty("name").GetString() ?? "未知游戏";
                            string url = game.GetProperty("url").GetString() ?? "";
                            
                            // 检查是否已经存在
                            if (!DownloadItems.Any(item => item.MagnetLink == url))
                            {
                                DownloadItems.Add(new DownloadItemViewModel(name, url));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError($"加载默认下载列表失败: {ex.Message}");
            }
        }

        private void LoadDownloadsFromConfig()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    var json = File.ReadAllText(_configPath);
                    var modelItems = JsonSerializer.Deserialize<ObservableCollection<DownloadItemModel>>(json);
                    if (modelItems != null)
                    {
                        var viewModels = modelItems.Select(model => new DownloadItemViewModel(model));
                        DownloadItems = new ObservableCollection<DownloadItemViewModel>(viewModels);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError($"加载下载配置失败: {ex.Message}");
            }
        }

        private void SaveDownloadsToConfig()
        {
            try
            {
                var modelItems = DownloadItems.Select(vm => vm.Model);
                var json = JsonSerializer.Serialize(modelItems, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_configPath, json);
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError($"保存下载配置失败: {ex.Message}");
            }
        }

        private void UpdateTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            // 在UI线程上更新
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                UpdateTotalStats();
            });
        }

        private void UpdateTotalStats()
        {
            var activeDownloads = DownloadItems.Where(item => item.IsDownloading).ToList();
            
            if (activeDownloads.Any())
            {
                double totalSpeed = activeDownloads.Sum(item => item.DownloadSpeed);
                TotalSpeed = FormatSpeed(totalSpeed);
                
                long totalDownloaded = DownloadItems.Sum(item => item.DownloadedBytes);
                long totalSize = DownloadItems.Sum(item => item.TotalBytes);
                
                if (totalSize > 0)
                {
                    TotalProgress = (double)totalDownloaded / totalSize * 100;
                }
                else
                {
                    TotalProgress = 0;
                }
            }
            else
            {
                TotalSpeed = "0 B/s";
                if (DownloadItems.All(item => item.IsCompleted))
                {
                    TotalProgress = 100;
                }
            }
        }


        private string FormatSpeed(double bytesPerSecond)
        {
            if (bytesPerSecond < 1024)
                return $"{bytesPerSecond:F1} B/s";
            else if (bytesPerSecond < 1024 * 1024)
                return $"{bytesPerSecond / 1024:F1} KB/s";
            else
                return $"{bytesPerSecond / (1024 * 1024):F1} MB/s";
        }

        private void StartAllDownloads()
        {
            if (!GlobalPaths.BTEnabled)
            {
                LoggerHelper.LogError("BT功能未启用，无法开始下载");
                return;
            }
            
            foreach (var item in DownloadItems.Where(x => !x.IsCompleted))
            {
                item.StartDownload();
            }
            IsAllPaused = false;
        }

        private void PauseAllDownloads()
        {
            foreach (var item in DownloadItems.Where(x => x.IsDownloading))
            {
                item.PauseDownload();
            }
            IsAllPaused = true;
        }

        private void RemoveCompletedDownloads()
        {
            var completedItems = DownloadItems.Where(x => x.IsCompleted).ToList();
            foreach (var item in completedItems)
            {
                DownloadItems.Remove(item);
            }
            SaveDownloadsToConfig();
        }

        private async Task AddDownload()
        {
            if (!GlobalPaths.BTEnabled)
            {
                LoggerHelper.LogError("BT功能未启用，无法添加下载");
                return;
            }
            
            try
            {
                var dialog = new Views.AddDownloadDialogWindow();
                
                // 获取主窗口作为父窗口
                var parentWindow = Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop 
                    ? desktop.MainWindow 
                    : null;
                
                if (parentWindow != null)
                {
                    var result = await dialog.ShowDialog<bool>(parentWindow);
                    
                    if (result == true && dialog.DataContext is ViewModels.AddDownloadDialogViewModel viewModel)
                    {
                        // 检查是否已经存在相同的磁力链接
                        if (!DownloadItems.Any(item => item.MagnetLink == viewModel.MagnetLink))
                        {
                            var newItem = new DownloadItemViewModel(viewModel.Name, viewModel.MagnetLink);
                            DownloadItems.Add(newItem);
                            SaveDownloadsToConfig();
                        }
                        else
                        {
                            // 可以添加一个提示，说明该下载已存在
                            LoggerHelper.LogInformation("该磁力链接已存在于下载列表中");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError($"添加下载失败: {ex.Message}");
            }
        }

        private void StartSelectedDownload()
        {
            if (!GlobalPaths.BTEnabled)
            {
                LoggerHelper.LogError("BT功能未启用，无法开始下载");
                return;
            }
            
            if (SelectedDownloadItem != null && !SelectedDownloadItem.IsCompleted)
            {
                SelectedDownloadItem.StartDownload();
            }
        }

        private void PauseSelectedDownload()
        {
            if (SelectedDownloadItem != null && SelectedDownloadItem.IsDownloading)
            {
                SelectedDownloadItem.PauseDownload();
            }
        }

        private void RemoveSelectedDownload()
        {
            if (SelectedDownloadItem != null)
            {
                DownloadItems.Remove(SelectedDownloadItem);
                SelectedDownloadItem = null;
                SaveDownloadsToConfig();
            }
        }

        protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            
            if (e.PropertyName == nameof(SelectedDownloadItem))
            {
                // 更新命令的可用性
                ((RelayCommand)StartSelectedCommand).NotifyCanExecuteChanged();
                ((RelayCommand)PauseSelectedCommand).NotifyCanExecuteChanged();
                ((RelayCommand)RemoveSelectedCommand).NotifyCanExecuteChanged();
            }
        }

        public void SaveConfig()
        {
            SaveDownloadsToConfig();
        }
    }
}
