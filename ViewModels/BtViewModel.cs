using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using ATC4_HQ.Services;
using System;

namespace ATC4_HQ.ViewModels
{
    public partial class BtViewModel : ObservableObject
    {
        private readonly BtService _btService = new BtService();

        [ObservableProperty]
        private double progress;

        [ObservableProperty]
        private string status = "未开始";

        [ObservableProperty]
        private string? torrentFilePath;

        [ObservableProperty]
        private string? downloadPath;

        [RelayCommand]
        public async Task StartDownloadAsync()
        {
            if (string.IsNullOrEmpty(TorrentFilePath) || string.IsNullOrEmpty(DownloadPath))
            {
                Status = "请先选择种子和下载目录";
                return;
            }
            Status = "初始化...";
            await _btService.InitAsync(DownloadPath);
            Status = "下载中...";
            await _btService.StartTorrentAsync(TorrentFilePath);
            Status = "下载/上传中...";
            // 可定时刷新进度
        }

        [RelayCommand]
        public async Task StopDownloadAsync()
        {
            await _btService.StopTorrentAsync();
            Status = "已停止";
        }

        public void RefreshProgress()
        {
            Progress = _btService.GetProgress();
        }
    }
}
