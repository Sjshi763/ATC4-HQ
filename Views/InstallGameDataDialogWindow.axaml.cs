using Avalonia.Controls;
using System;
using ATC4_HQ.ViewModels; // 引用您的 ViewModel 命名空间

namespace ATC4_HQ.Views
{
    public partial class InstallGameDataDialogWindow : Window
    {
        public InstallGameDataDialogWindow()
        {
            InitializeComponent();
            // 实例化 ViewModel 并设置为 DataContext
            // 这样 UserControl (InstallGameData) 就能自动继承这个 DataContext
            DataContext = new InstallGameDataViewModel();

            // 订阅 ViewModel 的 PropertyChanged 事件，监听 IsDialogOk 属性变化
            if (DataContext is InstallGameDataViewModel viewModel)
            {
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(InstallGameDataViewModel.IsDialogOk))
            {
                if (DataContext is InstallGameDataViewModel viewModel)
                {
                    // 当 IsDialogOk 属性被设置时，表示 ViewModel 已经决定关闭对话框
                    // 设置 DialogResult，并关闭窗口
                    this.Close(viewModel.IsDialogOk); // ShowDialog() 方法会返回这个 bool 值
                }
            }
        }

        // 可以重写 OnClosing 确保在窗口关闭时取消订阅事件，防止内存泄漏
        protected override void OnClosing(WindowClosingEventArgs e)
        {
            if (DataContext is InstallGameDataViewModel viewModel)
            {
                viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }
            base.OnClosing(e);
        }
    }
}