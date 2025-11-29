using System;

namespace ATC4_HQ.Models
{
    /// <summary>
    /// 下载项数据模型 - 纯数据模型，符合MVVM模式
    /// </summary>
    public class DownloadItemModel
    {
        public string Name { get; set; } = string.Empty;
        public string MagnetLink { get; set; } = string.Empty;
        public double Progress { get; set; }
        public long DownloadedBytes { get; set; }
        public long TotalBytes { get; set; }
        public double DownloadSpeed { get; set; }
        public string Status { get; set; } = "等待中";
        public bool IsDownloading { get; set; }
        public bool IsCompleted { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        
        public DownloadItemModel()
        {
            Progress = 0;
            DownloadedBytes = 0;
            TotalBytes = 0;
            DownloadSpeed = 0;
            Status = "等待中";
            IsDownloading = false;
            IsCompleted = false;
        }
        
        public DownloadItemModel(string name, string magnetLink) : this()
        {
            Name = name;
            MagnetLink = magnetLink;
        }
        
        /// <summary>
        /// 更新下载进度
        /// </summary>
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
        
        /// <summary>
        /// 设置错误状态
        /// </summary>
        public void SetError(string error)
        {
            ErrorMessage = error;
            Status = "错误";
            IsDownloading = false;
            IsCompleted = false;
        }
        
        /// <summary>
        /// 开始下载
        /// </summary>
        public void StartDownload()
        {
            IsDownloading = true;
            IsCompleted = false;
            Status = "开始下载";
            ErrorMessage = string.Empty;
        }
        
        /// <summary>
        /// 暂停下载
        /// </summary>
        public void PauseDownload()
        {
            IsDownloading = false;
            Status = "已暂停";
        }
        
        /// <summary>
        /// 格式化速度显示
        /// </summary>
        public string GetFormattedSpeed()
        {
            if (DownloadSpeed < 1024)
                return $"{DownloadSpeed:F1} B/s";
            else if (DownloadSpeed < 1024 * 1024)
                return $"{DownloadSpeed / 1024:F1} KB/s";
            else
                return $"{DownloadSpeed / (1024 * 1024):F1} MB/s";
        }
        
        /// <summary>
        /// 格式化大小显示
        /// </summary>
        public string GetFormattedSize()
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
