using MonoTorrent;
using MonoTorrent.Client;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace ATC4_HQ.ViewModels
{
    public class BtService
    {
        private ClientEngine? _engine;
        private readonly Dictionary<string, TorrentManager> _managers = new Dictionary<string, TorrentManager>();
        private string? _downloadPath;

        public event EventHandler<DownloadProgressEventArgs>? DownloadProgressChanged;
        public event EventHandler<DownloadCompletedEventArgs>? DownloadCompleted;

        public async Task<bool> InitAsync(string downloadPath)
        {
            _downloadPath = downloadPath;
            var settings = new EngineSettings();
            _engine = new ClientEngine(settings);
            return true;
        }

        public async Task<bool> StartTorrentAsync(string magnetLink, string gameName)
        {
            if (_engine == null || _downloadPath == null) return false;
            
            try
            {
                // 从磁力链接创建torrent
                var magnet = MagnetLink.Parse(magnetLink);
                
                // 为每个游戏创建单独的下载目录
                var gameDownloadPath = Path.Combine(_downloadPath, gameName);
                Directory.CreateDirectory(gameDownloadPath);
                
                // 直接添加磁力链接到引擎
                var manager = await _engine.AddAsync(magnet, gameDownloadPath);
                _managers[gameName] = manager;
                
                // 设置事件处理器
                manager.TorrentStateChanged += (s, e) => 
                {
                    if (e.NewState == TorrentState.Downloading)
                    {
                        StartProgressMonitoring(gameName, manager);
                    }
                };
                
                await manager.StartAsync();
                return true;
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError($"启动BT下载失败: {ex.Message}");
                return false;
            }
        }

        private async void StartProgressMonitoring(string gameName, TorrentManager manager)
        {
            while (manager.State == TorrentState.Downloading || manager.State == TorrentState.Seeding)
            {
                var progress = manager.Progress * 100;
                var downloadSpeed = manager.Monitor.DownloadRate;
                var uploaded = manager.Monitor.DataBytesUploaded;
                var downloaded = manager.Monitor.DataBytesDownloaded;
                var total = manager.Torrent?.Size ?? 0;

                DownloadProgressChanged?.Invoke(this, new DownloadProgressEventArgs
                {
                    GameName = gameName,
                    Progress = progress,
                    DownloadSpeed = downloadSpeed,
                    DownloadedBytes = downloaded,
                    TotalBytes = total
                });

                if (manager.Progress >= 1.0)
                {
                    DownloadCompleted?.Invoke(this, new DownloadCompletedEventArgs
                    {
                        GameName = gameName,
                        DownloadPath = manager.SavePath
                    });
                    break;
                }

                await Task.Delay(1000);
            }
        }

        public async Task StopTorrentAsync(string gameName)
        {
            if (_managers.TryGetValue(gameName, out var manager))
            {
                await manager.StopAsync();
            }
        }

        public async Task StopAllTorrentsAsync()
        {
            foreach (var manager in _managers.Values)
            {
                await manager.StopAsync();
            }
            _managers.Clear();
        }

        public double GetProgress(string gameName)
        {
            return _managers.TryGetValue(gameName, out var manager) ? manager.Progress * 100 : 0.0;
        }
    }

    public class DownloadProgressEventArgs : EventArgs
    {
        public string GameName { get; set; } = string.Empty;
        public double Progress { get; set; }
        public double DownloadSpeed { get; set; }
        public long DownloadedBytes { get; set; }
        public long TotalBytes { get; set; }
    }

    public class DownloadCompletedEventArgs : EventArgs
    {
        public string GameName { get; set; } = string.Empty;
        public string DownloadPath { get; set; } = string.Empty;
    }
}
