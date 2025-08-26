using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Diagnostics;
using System;
using Avalonia.Markup.Xaml;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia;
using Avalonia.Media;
using System.Net.Http;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

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
            var openWebsiteButton = this.FindControl<Button>("OpenWebsiteButton");
            var testInstallButton = this.FindControl<Button>("TestInstallButton");
            var backButton = this.FindControl<Button>("BackButton");
            
            if (openWebsiteButton != null)
                openWebsiteButton.Click += OpenWebsiteButton_Click;
            if (testInstallButton != null)
                testInstallButton.Click += TestInstallButton_Click;
            if (backButton != null)
                backButton.Click += BackButton_Click;
        }

        private async void OpenWebsiteButton_Click(object? sender, RoutedEventArgs e)
        {
            // 自动下载、解压并运行OpenAL安装程序
            await DownloadAndInstallOpenAL();
        }

        private async Task DownloadAndInstallOpenAL()
        {
            string downloadUrl = "https://www.openal.org/downloads/oalinst.zip";
            string tempPath = Path.GetTempPath();
            string zipFilePath = Path.Combine(tempPath, "oalinst.zip");
            string extractPath = Path.Combine(tempPath, "OpenAL");

            try
            {
                // 下载文件
                using (HttpClient client = new HttpClient())
                {
                    Console.WriteLine("开始下载OpenAL安装包...");
                    var response = await client.GetAsync(downloadUrl);
                    response.EnsureSuccessStatusCode();
                    
                    var content = await response.Content.ReadAsByteArrayAsync();
                    await File.WriteAllBytesAsync(zipFilePath, content);
                    Console.WriteLine("下载完成");
                }

                // 解压文件
                if (Directory.Exists(extractPath))
                {
                    Directory.Delete(extractPath, true);
                }
                Directory.CreateDirectory(extractPath);
                
                Console.WriteLine("开始解压...");
                ZipFile.ExtractToDirectory(zipFilePath, extractPath);
                Console.WriteLine("解压完成");

                // 查找并运行exe文件
                string[] exeFiles = Directory.GetFiles(extractPath, "*.exe", SearchOption.AllDirectories);
                if (exeFiles.Length > 0)
                {
                    string exePath = exeFiles[0]; // 假设第一个exe文件就是安装程序
                    Console.WriteLine($"找到安装程序: {exePath}");
                    
                    // 运行安装程序
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = exePath,
                        UseShellExecute = true
                    });
                    Console.WriteLine("已启动OpenAL安装程序");
                }
                else
                {
                    Console.WriteLine("未找到exe安装文件");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"安装过程中发生错误: {ex.Message}");
                // 如果自动安装失败，还是打开网站
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "https://www.openal.org/",
                        UseShellExecute = true
                    });
                }
                catch (Exception webEx)
                {
                    Console.WriteLine($"无法打开网站: {webEx.Message}");
                }
            }
            finally
            {
                // 清理临时文件
                try
                {
                    if (File.Exists(zipFilePath))
                        File.Delete(zipFilePath);
                }
                catch (Exception cleanupEx)
                {
                    Console.WriteLine($"清理临时文件时发生错误: {cleanupEx.Message}");
                }
            }
        }

        private void TestInstallButton_Click(object? sender, RoutedEventArgs e)
        {
            // 测试安装逻辑
            System.Console.WriteLine("用户点击测试安装按钮");
            
            // 检查OpenAL是否已安装
            if (IsOpenALInstalled())
            {
                // 显示安装成功的消息
                ShowTestResult("测试成功", "OpenAL库已正确安装！", true);
            }
            else
            {
                // 显示安装失败的消息
                ShowTestResult("测试失败", "OpenAL库未安装或安装不完整！", false);
            }
        }

        private bool IsOpenALInstalled()
        {
            // 检查系统目录中是否存在openal32.dll
            string openalPath = Path.Combine(Environment.SystemDirectory, "openal32.dll");
            return File.Exists(openalPath);
        }

        private void ShowTestResult(string title, string message, bool isSuccess)
        {
            // 创建测试结果对话框
            var resultWindow = new Window
            {
                Title = title,
                Width = 400,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                CanResize = false
            };

            var stackPanel = new StackPanel { Margin = new Thickness(20) };

            var titleTextBlock = new TextBlock
            {
                Text = title,
                FontSize = 20,
                FontWeight = Avalonia.Media.FontWeight.Bold,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            };

            var messageTextBlock = new TextBlock
            {
                Text = message,
                FontSize = 16,
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };

            var okButton = new Button
            {
                Content = "确定",
                Width = 80,
                Height = 35,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            };

            okButton.Click += (s, args) => resultWindow.Close();

            stackPanel.Children.Add(titleTextBlock);
            stackPanel.Children.Add(messageTextBlock);
            stackPanel.Children.Add(okButton);

            resultWindow.Content = stackPanel;
            
            // 显示对话框
            var parentWindow = this.VisualRoot as Window;
            if (parentWindow != null)
            {
                resultWindow.ShowDialog(parentWindow);
            }
        }

        private void BackButton_Click(object? sender, RoutedEventArgs e)
        {
            // 返回逻辑
            System.Console.WriteLine("用户选择返回");
            // 关闭当前窗口
            var window = this.VisualRoot as Window;
            if (window != null)
            {
                window.Close();
            }
        }
    }
}
