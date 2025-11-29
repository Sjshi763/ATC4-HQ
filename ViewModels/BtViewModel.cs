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
        private string? magnetLink;

        [ObservableProperty]
        private string? downloadPath;

        [ObservableProperty]
        private string? gameName;

        [RelayCommand]
        public async Task StartDownloadAsync()
        {
            if (string.IsNullOrEmpty(MagnetLink) || string.IsNullOrEmpty(DownloadPath) || string.IsNullOrEmpty(GameName))
            {
                Status = "请先选择磁力链接、下载目录和游戏名称";
                return;
            }
            Status = "初始化...";
            await _btService.InitAsync(DownloadPath);
            Status = "下载中...";
            await _btService.StartTorrentAsync(MagnetLink, GameName);
            Status = "下载/上传中...";
            // 可定时刷新进度
        }

        [RelayCommand]
        public async Task StopDownloadAsync()
        {
            if (string.IsNullOrEmpty(GameName))
            {
                Status = "没有活动的下载";
                return;
            }
            await _btService.StopTorrentAsync(GameName);
            Status = "已停止";
        }

        public void RefreshProgress()
        {
            Progress = string.IsNullOrEmpty(GameName) ? 0.0 : _btService.GetProgress(GameName);
        }
    }
}
