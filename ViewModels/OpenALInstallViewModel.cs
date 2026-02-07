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
using Microsoft.Extensions.Logging;

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

            LoggerHelper.LogInformation("=== OpenAL安装流程开始 ===");
            IsInstalling = true;
            InstallStatus = "开始下载OpenAL安装包...";
            LoggerHelper.LogInformation("[OpenAL安装] 开始安装OpenAL");

            string downloadUrl = "https://www.openal.org/downloads/oalinst.zip";
            string tempPath = Path.GetTempPath();
            string zipFilePath = Path.Combine(tempPath, "oalinst.zip");
            string extractPath = Path.Combine(tempPath, "OpenAL");
            
            LoggerHelper.LogInformation($"[OpenAL安装] 下载URL: {downloadUrl}");
            LoggerHelper.LogInformation($"[OpenAL安装] 临时文件路径: {zipFilePath}");
            LoggerHelper.LogInformation($"[OpenAL安装] 解压路径: {extractPath}");

            try
            {
                // 下载文件
                InstallStatus = "正在下载...";
                LoggerHelper.LogInformation("[OpenAL安装] 开始下载文件...");
                using (HttpClient client = new HttpClient())
                {
                    LoggerHelper.LogInformation("[OpenAL安装] 创建HTTP客户端");
                    var response = await client.GetAsync(downloadUrl);
                    LoggerHelper.LogInformation($"[OpenAL安装] HTTP响应状态码: {response.StatusCode}");
                    response.EnsureSuccessStatusCode();
                    LoggerHelper.LogInformation("[OpenAL安装] HTTP请求成功");
                    
                    var content = await response.Content.ReadAsByteArrayAsync();
                    LoggerHelper.LogInformation($"[OpenAL安装] 下载内容大小: {content.Length} 字节");
                    await File.WriteAllBytesAsync(zipFilePath, content);
                    InstallStatus = "下载完成";
                    LoggerHelper.LogInformation("[OpenAL安装] 文件下载完成并保存");
                }

                // 解压文件
                InstallStatus = "正在解压...";
                LoggerHelper.LogInformation("[OpenAL安装] 开始解压文件...");
                if (Directory.Exists(extractPath))
                {
                    LoggerHelper.LogInformation($"[OpenAL安装] 删除已存在的解压目录: {extractPath}");
                    Directory.Delete(extractPath, true);
                }
                LoggerHelper.LogInformation($"[OpenAL安装] 创建解压目录: {extractPath}");
                Directory.CreateDirectory(extractPath);
                
                LoggerHelper.LogInformation("[OpenAL安装] 执行解压操作...");
                ZipFile.ExtractToDirectory(zipFilePath, extractPath);
                InstallStatus = "解压完成";
                LoggerHelper.LogInformation("[OpenAL安装] 文件解压完成");

                // 查找并运行exe文件
                LoggerHelper.LogInformation("[OpenAL安装] 搜索EXE安装文件...");
                string[] exeFiles = Directory.GetFiles(extractPath, "*.exe", SearchOption.AllDirectories);
                LoggerHelper.LogInformation($"[OpenAL安装] 找到 {exeFiles.Length} 个EXE文件");
                
                if (exeFiles.Length > 0)
                {
                    string exePath = exeFiles[0]; // 假设第一个exe文件就是安装程序
                    InstallStatus = $"找到安装程序: {Path.GetFileName(exePath)}";
                    LoggerHelper.LogInformation($"[OpenAL安装] 选择的安装程序: {exePath}");
                    
                    // 运行安装程序
                    LoggerHelper.LogInformation("[OpenAL安装] 启动安装程序...");
                    try
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = exePath,
                            UseShellExecute = true
                        });
                        InstallStatus = "已启动OpenAL安装程序，请按照安装向导完成安装";
                        LoggerHelper.LogInformation("[OpenAL安装] 安装程序启动成功");
                    }
                    catch (Exception processEx)
                    {
                        LoggerHelper.LogError($"[OpenAL安装] 启动安装程序失败: {processEx.Message}");
                        ShowMessage?.Invoke(this, $"启动安装程序失败: {processEx.Message}");
                    }
                }
                else
                {
                    LoggerHelper.LogError("[OpenAL安装] 未找到EXE安装文件");
                    ShowMessage?.Invoke(this, "未找到exe安装文件");
                }
            }
            catch (Exception ex)
            {
                InstallStatus = $"安装过程中发生错误: {ex.Message}";
                LoggerHelper.LogError($"[OpenAL安装错误] {ex.Message}");
                LoggerHelper.LogError($"[OpenAL安装错误] 异常类型: {ex.GetType().Name}");
                LoggerHelper.LogError($"[OpenAL安装错误] 堆栈跟踪: {ex.StackTrace}");
                
                // 如果自动安装失败，还是打开网站
                LoggerHelper.LogInformation("[OpenAL安装] 自动安装失败，尝试打开官方网站...");
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "https://www.openal.org/",
                        UseShellExecute = true
                    });
                    LoggerHelper.LogInformation("[OpenAL安装] 官方网站打开成功");
                }
                catch (Exception webEx)
                {
                    LoggerHelper.LogError($"[OpenAL安装] 打开官方网站失败: {webEx.Message}");
                    ShowMessage?.Invoke(this, $"无法打开网站: {webEx.Message}");
                }
            }
            finally
            {
                // 清理临时文件
                LoggerHelper.LogInformation("[OpenAL安装] 开始清理临时文件...");
                try
                {
                    if (File.Exists(zipFilePath))
                    {
                        LoggerHelper.LogInformation($"[OpenAL安装] 删除ZIP文件: {zipFilePath}");
                        File.Delete(zipFilePath);
                    }
                    if (Directory.Exists(extractPath))
                    {
                        LoggerHelper.LogInformation($"[OpenAL安装] 删除解压目录: {extractPath}");
                        Directory.Delete(extractPath, true);
                    }
                    LoggerHelper.LogInformation("[OpenAL安装] 临时文件清理完成");
                }
                catch (Exception cleanupEx)
                {
                    LoggerHelper.LogError($"[OpenAL安装清理错误] {cleanupEx.Message}");
                }
                
                IsInstalling = false;
                LoggerHelper.LogInformation("=== OpenAL安装流程结束 ===");
            }
        }

        private void TestInstall()
        {
            // 测试安装逻辑
            LoggerHelper.LogInformation("=== OpenAL安装测试开始 ===");
            LoggerHelper.LogInformation("[OpenAL测试] 用户点击测试安装按钮");
            
            // 检查OpenAL是否已安装
            LoggerHelper.LogInformation("[OpenAL测试] 检查OpenAL安装状态...");
            bool isInstalled = IsOpenALInstalled();
            LoggerHelper.LogInformation($"[OpenAL测试] 安装状态: {(isInstalled ? "已安装" : "未安装")}");
            
            if (isInstalled)
            {
                // 显示安装成功的消息
                LoggerHelper.LogInformation("[OpenAL测试] 测试结果: 成功");
                ShowMessage?.Invoke(this, "测试成功\nOpenAL库已正确安装！");
            }
            else
            {
                // 显示安装失败的消息
                LoggerHelper.LogInformation("[OpenAL测试] 测试结果: 失败");
                ShowMessage?.Invoke(this, "测试失败\nOpenAL库未安装或安装不完整！");
            }
            LoggerHelper.LogInformation("=== OpenAL安装测试结束 ===");
        }

        private bool IsOpenALInstalled()
        {
            // 检查系统目录中是否存在openal32.dll
            string openalPath = Path.Combine(Environment.SystemDirectory, "openal32.dll");
            LoggerHelper.LogInformation($"[OpenAL检查] 检查文件: {openalPath}");
            bool exists = File.Exists(openalPath);
            LoggerHelper.LogInformation($"[OpenAL检查] 文件存在: {exists}");
            return exists;
        }

        private void OnBack()
        {
            // 返回逻辑
            LoggerHelper.LogInformation("[OpenAL安装] 用户选择返回");
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }
}
