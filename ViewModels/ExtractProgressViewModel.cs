using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ATC4_HQ.ViewModels
{
    public partial class ExtractProgressViewModel : ObservableObject
    {
        [ObservableProperty]
        private double progress;

        [ObservableProperty]
        private string statusMessage = "准备中...";

        [ObservableProperty]
        private bool canCancel = true;

        [ObservableProperty]
        private bool isCompleted;

        [ObservableProperty]
        private bool isCancelled;

        private string _stepDetail = "等待开始...";

        public string StepDetail
        {
            get => _stepDetail;
            set => SetProperty(ref _stepDetail, value);
        }

        public ObservableCollection<string> LogMessages { get; } = new ObservableCollection<string>();

        public CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();

        public event EventHandler? CancelRequested;
        public event EventHandler? Completed;

        public ExtractProgressViewModel()
        {
        }

        public void UpdateProgress(double newProgress, string? message = null)
        {
            Progress = Math.Clamp(newProgress, 0.0, 100.0);
            if (!string.IsNullOrEmpty(message))
            {
                StatusMessage = message;
            }
        }

        public void UpdateStepProgress(int completedSteps, int totalSteps, string message)
        {
            int safeTotalSteps = Math.Max(totalSteps, 1);
            int safeCompletedSteps = Math.Clamp(completedSteps, 0, safeTotalSteps);

            Progress = (safeCompletedSteps * 100.0) / safeTotalSteps;
            StatusMessage = message;
            StepDetail = $"步骤 {safeCompletedSteps}/{safeTotalSteps}";
        }

        public void AddLog(string message)
        {
            LogMessages.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
        }

        [RelayCommand]
        private void Cancel()
        {
            if (!CanCancel || IsCompleted) return;
            
            IsCancelled = true;
            CanCancel = false;
            StatusMessage = "正在取消...";
            AddLog("用户请求取消解压操作");
            CancellationTokenSource.Cancel();
            CancelRequested?.Invoke(this, EventArgs.Empty);
        }

        public void OnExtractionCompleted()
        {
            IsCompleted = true;
            CanCancel = false;
            StatusMessage = "解压完成！";
            StepDetail = "全部步骤完成";
            Progress = 100.0;
            AddLog("解压操作已完成");
            Completed?.Invoke(this, EventArgs.Empty);
        }

        public void OnExtractionFailed(string error)
        {
            IsCompleted = true;
            CanCancel = false;
            StepDetail = "执行失败";
            StatusMessage = $"解压失败：{error}";
            AddLog($"错误：{error}");
        }

        protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
        }
    }
}