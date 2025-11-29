using CommunityToolkit.Mvvm.ComponentModel;
using ATC4_HQ.Models;
using master.Globals;
using ATC4_HQ.Services;

namespace ATC4_HQ.ViewModels
{
    /// <summary>
    /// 下载项视图模型 - 符合MVVM模式，负责UI绑定和业务逻辑
    /// </summary>
    public partial class DownloadItemViewModel : ViewModelBase
    {
        private readonly DownloadItemModel _model;
        private static BtService? _btService;

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

        public DownloadItemModel Model => _model;

        public static void SetBtService(BtService btService)
        {
            _btService = btService;
        }

        public DownloadItemViewModel(DownloadItemModel model)
        {
            _model = model;
            UpdateFromModel();
        }

        public DownloadItemViewModel(string name, string magnetLink)
        {
            _model = new DownloadItemModel(name, magnetLink);
            UpdateFromModel();
        }

        /// <summary>
        /// 从模型更新属性
        /// </summary>
        private void UpdateFromModel()
        {
            Name = _model.Name;
            MagnetLink = _model.MagnetLink;
            Progress = _model.Progress;
            DownloadedBytes = _model.DownloadedBytes;
            TotalBytes = _model.TotalBytes;
            DownloadSpeed = _model.DownloadSpeed;
            Status = _model.Status;
            IsDownloading = _model.IsDownloading;
            IsCompleted = _model.IsCompleted;
            ErrorMessage = _model.ErrorMessage;
        }

        /// <summary>
        /// 更新下载进度
        /// </summary>
        public void UpdateProgress(double progress, long downloadedBytes, long totalBytes, double speed)
        {
            _model.UpdateProgress(progress, downloadedBytes, totalBytes, speed);
            UpdateFromModel();
        }

        /// <summary>
        /// 设置错误状态
        /// </summary>
        public void SetError(string error)
        {
            _model.SetError(error);
            UpdateFromModel();
        }

        /// <summary>
        /// 开始下载
        /// </summary>
        public async void StartDownload()
        {
            if (!GlobalPaths.BTEnabled)
            {
                SetError("BT功能未启用，无法开始下载");
                return;
            }

            if (_btService == null)
            {
                SetError("BT服务未初始化");
                return;
            }
            
            _model.StartDownload();
            UpdateFromModel();

            // 调用真正的BT服务开始下载
            var success = await _btService.StartTorrentAsync(MagnetLink, Name);
            if (!success)
            {
                SetError("启动BT下载失败");
            }
        }

        /// <summary>
        /// 暂停下载
        /// </summary>
        public async void PauseDownload()
        {
            if (_btService == null)
            {
                SetError("BT服务未初始化");
                return;
            }

            await _btService.StopTorrentAsync(Name);
            _model.PauseDownload();
            UpdateFromModel();
        }

        /// <summary>
        /// 格式化速度显示
        /// </summary>
        public string FormattedSpeed => _model.GetFormattedSpeed();

        /// <summary>
        /// 格式化大小显示
        /// </summary>
        public string FormattedSize => _model.GetFormattedSize();
    }
}
