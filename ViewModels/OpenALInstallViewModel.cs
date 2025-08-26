using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.IO;
using System.Net.Http;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia;
using System.Collections.Generic;

namespace ATC4_HQ.ViewModels
{
    public partial class OpenALInstallViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _title = "OPENAL 安装";

        [ObservableProperty]
        private string _description = "检测到系统未安装 OPENAL 库，这是运行游戏所必需的组件。";

        [ObservableProperty]
        private string _installInstructions = "安装：";

        [ObservableProperty]
        private List<string> _installationSteps = new()
        {
            "1. 点击下方按钮自动下载安装包",
            "2. 程序将自动解压并运行安装程序",
            "3. 按照安装向导完成安装"
        };

        [ObservableProperty]
        private string _downloadButtonText = "下载并安装";

        [ObservableProperty]
        private string _testButtonText = "测试安装";

        [ObservableProperty]
        private string _backButtonText = "返回";

        [ObservableProperty]
        private bool _isInstalling = false;

        [ObservableProperty]
        private string _installStatus = "";

        public IRelayCommand DownloadAndInstallCommand { get; }
        public IRelayCommand TestInstallCommand { get; }
        public IRelayCommand BackCommand { get; }

        public event EventHandler? RequestClose;
        public event EventHandler<string>? ShowMessage;

        public OpenALInstallViewModel()
        {
            DownloadAndInstallCommand = new RelayCommand(async () => await DownloadAndInstallOpenALAsync());
            TestInstallCommand = new RelayCommand(TestInstall);
            BackCommand = new RelayCommand(OnBack);
        }

        private async Task DownloadAndInstallOpenALAsync()
        {
            if (IsInstalling) return;

            IsInstalling = true;
            InstallStatus = "开始下载OpenAL安装包...";

            string downloadUrl = "https://www.openal.org/downloads/oalinst.zip";
            string tempPath = Path.GetTempPath();
            string zipFilePath = Path.Combine(tempPath, "oalinst.zip");
            string extractPath = Path.Combine(tempPath, "OpenAL");

            try
            {
                // 下载文件
                using (HttpClient client = new HttpClient())
                {
                    InstallStatus = "正在下载...";
                    var response = await client.GetAsync(downloadUrl);
                    response.EnsureSuccessStatusCode();
                    
                    var content = await response.Content.ReadAsByteArrayAsync();
                    await File.WriteAllBytesAsync(zipFilePath, content);
                    InstallStatus = "下载完成";
                }

                // 解压文件
                InstallStatus = "正在解压...";
                if (Directory.Exists(extractPath))
                {
                    Directory.Delete(extractPath, true);
                }
                Directory.CreateDirectory(extractPath);
                
                ZipFile.ExtractToDirectory(zipFilePath, extractPath);
                InstallStatus = "解压完成";

                // 查找并运行exe文件
                string[] exeFiles = Directory.GetFiles(extractPath, "*.exe", SearchOption.AllDirectories);
                if (exeFiles.Length > 0)
                {
                    string exePath = exeFiles[0]; // 假设第一个exe文件就是安装程序
                    InstallStatus = $"找到安装程序: {Path.GetFileName(exePath)}";
                    
                    // 运行安装程序
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = exePath,
                        UseShellExecute = true
                    });
                    InstallStatus = "已启动OpenAL安装程序，请按照安装向导完成安装";
                }
                else
                {
                    ShowMessage?.Invoke(this, "未找到exe安装文件");
                }
            }
            catch (Exception ex)
            {
                InstallStatus = $"安装过程中发生错误: {ex.Message}";
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
                    ShowMessage?.Invoke(this, $"无法打开网站: {webEx.Message}");
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
                
                IsInstalling = false;
            }
        }

        private void TestInstall()
        {
            // 测试安装逻辑
            System.Console.WriteLine("用户点击测试安装按钮");
            
            // 检查OpenAL是否已安装
            if (IsOpenALInstalled())
            {
                // 显示安装成功的消息
                ShowMessage?.Invoke(this, "测试成功\nOpenAL库已正确安装！");
            }
            else
            {
                // 显示安装失败的消息
                ShowMessage?.Invoke(this, "测试失败\nOpenAL库未安装或安装不完整！");
            }
        }

        private bool IsOpenALInstalled()
        {
            // 检查系统目录中是否存在openal32.dll
            string openalPath = Path.Combine(Environment.SystemDirectory, "openal32.dll");
            return File.Exists(openalPath);
        }

        private void OnBack()
        {
            // 返回逻辑
            System.Console.WriteLine("用户选择返回");
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }
}
