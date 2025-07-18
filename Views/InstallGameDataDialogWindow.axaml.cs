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
            var viewModel = new InstallGameDataViewModel();
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
            DataContext = viewModel;
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(InstallGameDataViewModel.ShouldClose))
            {
                if (DataContext is InstallGameDataViewModel viewModel && viewModel.ShouldClose)
                {
                    Close(viewModel.IsDialogOk);
                }
            }
        }
    }
}