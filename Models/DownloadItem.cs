using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace ATC4_HQ.Models
{
    public partial class DownloadItem : ObservableObject
    {
        [ObservableProperty]
        private string _name = string.Empty;
        
        [ObservableProperty]
        private string _magnetLink = string.Empty;
        
        [ObservableProperty]
        private double _progress;
        
        [ObservableProperty]
        private long _downloadedBytes;
        
        [ObservableProperty]
        private long _totalBytes;
        
        [ObservableProperty]
        private double _downloadSpeed;
        
        [ObservableProperty]
        private string _status = "等待中";
        
        [ObservableProperty]
        private bool _isDownloading;
        
        [ObservableProperty]
        private bool _isCompleted;
        
        [ObservableProperty]
        private string _errorMessage = string.Empty;
        
        public DownloadItem(string name, string magnetLink)
        {
            Name = name;
            MagnetLink = magnetLink;
            Progress = 0;
            DownloadedBytes = 0;
            TotalBytes = 0;
            DownloadSpeed = 0;
            Status = "等待中";
            IsDownloading = false;
            IsCompleted = false;
        }
        
        public void UpdateProgress(double progress, long downloadedBytes, long totalBytes, double speed)
        {
            Progress = progress;
            DownloadedBytes = downloadedBytes;
            TotalBytes = totalBytes;
            DownloadSpeed = speed;
            
            if (progress >= 100)
            {
                Status = "已完成";
                IsDownloading = false;
                IsCompleted = true;
            }
            else if (IsDownloading)
            {
                Status = "下载中";
            }
        }
        
        public void SetError(string error)
        {
            ErrorMessage = error;
            Status = "错误";
            IsDownloading = false;
            IsCompleted = false;
        }
        
        public void StartDownload()
        {
            IsDownloading = true;
            IsCompleted = false;
            Status = "开始下载";
            ErrorMessage = string.Empty;
        }
        
        public void PauseDownload()
        {
            IsDownloading = false;
            Status = "已暂停";
        }
        
        public string FormattedSpeed
        {
            get
            {
                if (DownloadSpeed < 1024)
                    return $"{DownloadSpeed:F1} B/s";
                else if (DownloadSpeed < 1024 * 1024)
                    return $"{DownloadSpeed / 1024:F1} KB/s";
                else
                    return $"{DownloadSpeed / (1024 * 1024):F1} MB/s";
            }
        }
        
        public string FormattedSize
        {
            get
            {
                string FormatBytes(long bytes)
                {
                    if (bytes < 1024)
                        return $"{bytes} B";
                    else if (bytes < 1024 * 1024)
                        return $"{bytes / 1024:F1} KB";
                    else if (bytes < 1024 * 1024 * 1024)
                        return $"{bytes / (1024 * 1024):F1} MB";
                    else
                        return $"{bytes / (1024 * 1024 * 1024):F1} GB";
                }
                
                return $"{FormatBytes(DownloadedBytes)} / {FormatBytes(TotalBytes)}";
            }
        }
    }
}
