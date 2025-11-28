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
using master.Globals;

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

            LoadDownloadsFromConfig();
            LoadDefaultDownloads();
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
                Console.WriteLine($"加载默认下载列表失败: {ex.Message}");
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
                Console.WriteLine($"加载下载配置失败: {ex.Message}");
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
                Console.WriteLine($"保存下载配置失败: {ex.Message}");
            }
        }

        private void UpdateTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            // 在UI线程上更新
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                UpdateTotalStats();
                SimulateDownloadProgress();
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

        private void SimulateDownloadProgress()
        {
            var random = new Random();
            
            foreach (var item in DownloadItems.Where(x => x.IsDownloading && !x.IsCompleted))
            {
                // 模拟下载进度
                double progressIncrement = random.NextDouble() * 2; // 0-2%的进度增长
                double speed = random.NextDouble() * 1024 * 1024; // 0-1MB/s的速度
                
                long newProgress = (long)(item.TotalBytes * (item.Progress + progressIncrement) / 100);
                long downloadedBytes = Math.Min(newProgress, item.TotalBytes);
                
                item.UpdateProgress(
                    Math.Min(item.Progress + progressIncrement, 100),
                    downloadedBytes,
                    item.TotalBytes,
                    speed
                );
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
                Console.WriteLine("BT功能未启用，无法开始下载");
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
                Console.WriteLine("BT功能未启用，无法添加下载");
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
                            Console.WriteLine("该磁力链接已存在于下载列表中");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"添加下载失败: {ex.Message}");
            }
        }

        private void StartSelectedDownload()
        {
            if (!GlobalPaths.BTEnabled)
            {
                Console.WriteLine("BT功能未启用，无法开始下载");
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
