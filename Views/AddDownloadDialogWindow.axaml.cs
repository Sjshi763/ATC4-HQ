using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ATC4_HQ.ViewModels;
using System;
using System.Threading.Tasks;

namespace ATC4_HQ.Views
{
    public partial class AddDownloadDialogWindow : Window
    {
        public AddDownloadDialogWindow()
        {
            InitializeComponent();
            
            // 设置ViewModel
            DataContext = new AddDownloadDialogViewModel();
            
            // 绑定按钮事件
            CancelButton.Click += OnCancelClick;
            ConfirmButton.Click += OnConfirmClick;
        }

        private void OnCancelClick(object? sender, RoutedEventArgs e)
        {
            Close(false);
        }

        private async void OnConfirmClick(object? sender, RoutedEventArgs e)
        {
            if (DataContext is AddDownloadDialogViewModel viewModel)
            {
                if (viewModel.ValidateInput())
                {
                    Close(true);
                }
                else
                {
                    // 显示错误提示
                    await ShowErrorDialog(viewModel.ErrorMessage);
                }
            }
        }

        private async Task ShowErrorDialog(string message)
        {
            // 简单的错误提示，使用Avalonia内置的对话框
            var errorDialog = new Window
            {
                Title = "输入错误",
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false
            };

            var stackPanel = new StackPanel
            {
                Margin = new Thickness(20),
                Spacing = 15
            };

            var textBlock = new TextBlock
            {
                Text = message,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap
            };

            var button = new Button
            {
                Content = "确定",
                Width = 80,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right
            };
            button.Click += (s, e) => errorDialog.Close();

            stackPanel.Children.Add(textBlock);
            stackPanel.Children.Add(button);
            errorDialog.Content = stackPanel;

            await errorDialog.ShowDialog(this);
        }
    }
}
