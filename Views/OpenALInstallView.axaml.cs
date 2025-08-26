using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Diagnostics;
using System;
using Avalonia.Markup.Xaml;

namespace ATC4_HQ.Views
{
    public partial class OpenALInstallView : UserControl
    {
        public OpenALInstallView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            
            // 绑定按钮事件
            var autoInstallButton = this.FindControl<Button>("AutoInstallButton");
            var openWebsiteButton = this.FindControl<Button>("OpenWebsiteButton");
            var backButton = this.FindControl<Button>("BackButton");
            
            if (autoInstallButton != null)
                autoInstallButton.Click += AutoInstallButton_Click;
            if (openWebsiteButton != null)
                openWebsiteButton.Click += OpenWebsiteButton_Click;
            if (backButton != null)
                backButton.Click += BackButton_Click;
        }

        private void AutoInstallButton_Click(object? sender, RoutedEventArgs e)
        {
            // 自动安装逻辑
            System.Console.WriteLine("开始自动安装OPENAL...");
            // 这里可以添加自动下载和安装的逻辑
        }

        private void OpenWebsiteButton_Click(object? sender, RoutedEventArgs e)
        {
            // 打开OPENAL官方网站
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://www.openal.org/",
                    UseShellExecute = true
                });
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"无法打开网站: {ex.Message}");
            }
        }

        private void BackButton_Click(object? sender, RoutedEventArgs e)
        {
            // 返回逻辑
            System.Console.WriteLine("用户选择返回");
            // 这里可以添加返回到主界面的逻辑
        }
    }
}
