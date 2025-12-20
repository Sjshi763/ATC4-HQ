using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO.Compression;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Extensions.Logging;

namespace ATC4_HQ.ViewModels
{
    public partial class LEInstallViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _title = "Locale Emulator 安装";

        [ObservableProperty]
        private string _description = "检测到系统未安装 Locale Emulator，这是运行游戏所必需的组件。";

        [ObservableProperty]
        private string _installInstructions = "安装：";

        [ObservableProperty]
        private List<string> _installationSteps = new()
        {
            "1. 点击下方按钮自动下载最新版本",
            "2. 选择保存位置并自动解压",
            "3. 自动配置Locale Emulator路径"
        };

        [ObservableProperty]
        private string _downloadButtonText = "自动下载安装";


        [ObservableProperty]
        private string _backButtonText = "返回";

        [ObservableProperty]
        private bool _isInstalling = false;

        [ObservableProperty]
        private string _installStatus = "";

        public IRelayCommand DownloadAndInstallCommand { get; }
        public IRelayCommand BackCommand { get; }

        public event EventHandler? RequestClose;
        public event EventHandler<string>? ShowMessage;

        public LEInstallViewModel()
        {
            DownloadAndInstallCommand = new RelayCommand(async () => await DownloadAndInstallLEAsync());
            BackCommand = new RelayCommand(OnBack);
        }

        private async Task DownloadAndInstallLEAsync()
        {
            if (IsInstalling) return;

            LoggerHelper.LogInformation("=== LE安装流程开始 ===");
            IsInstalling = true;
            InstallStatus = "开始下载Locale Emulator最新版本...";
            LoggerHelper.LogInformation("[LE安装] 开始安装Locale Emulator");

            try
            {
                // GitHub仓库信息
                string repoOwner = "xupefei";
                string repoName = "Locale-Emulator";
                string downloadUrl = $"https://github.com/{repoOwner}/{repoName}/releases/latest/download/Locale.Emulator.2.5.0.1.zip";
                LoggerHelper.LogInformation($"[LE安装] 下载URL: {downloadUrl}");
                
                // 获取临时目录
                string tempPath = Path.GetTempPath();
                string zipFilePath = Path.Combine(tempPath, "LocaleEmulator.zip");
                string extractPath = Path.Combine(tempPath, "LocaleEmulator");
                LoggerHelper.LogInformation($"[LE安装] 临时文件路径: {zipFilePath}");
                LoggerHelper.LogInformation($"[LE安装] 解压路径: {extractPath}");

                // 下载文件
                InstallStatus = "正在下载...";
                LoggerHelper.LogInformation("[LE安装] 开始下载文件...");
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(10); // 设置10分钟超时
                    LoggerHelper.LogInformation("[LE安装] HTTP客户端超时设置: 10分钟");
                    
                    var response = await client.GetAsync(downloadUrl);
                    LoggerHelper.LogInformation($"[LE安装] HTTP响应状态码: {response.StatusCode}");
                    response.EnsureSuccessStatusCode();
                    LoggerHelper.LogInformation("[LE安装] HTTP请求成功");
                    
                    var content = await response.Content.ReadAsByteArrayAsync();
                    LoggerHelper.LogInformation($"[LE安装] 下载内容大小: {content.Length} 字节");
                    await File.WriteAllBytesAsync(zipFilePath, content);
                    InstallStatus = "下载完成";
                    LoggerHelper.LogInformation("[LE安装] 文件下载完成并保存");
                }

                // 解压文件
                InstallStatus = "正在解压...";
                LoggerHelper.LogInformation("[LE安装] 开始解压文件...");
                if (Directory.Exists(extractPath))
                {
                    LoggerHelper.LogInformation($"[LE安装] 删除已存在的解压目录: {extractPath}");
                    Directory.Delete(extractPath, true);
                }
                LoggerHelper.LogInformation($"[LE安装] 创建解压目录: {extractPath}");
                Directory.CreateDirectory(extractPath);
                
                LoggerHelper.LogInformation("[LE安装] 执行解压操作...");
                ZipFile.ExtractToDirectory(zipFilePath, extractPath);
                InstallStatus = "解压完成";
                LoggerHelper.LogInformation("[LE安装] 文件解压完成");

                // 让用户选择保存位置
                LoggerHelper.LogInformation("[LE安装] 准备显示保存位置对话框...");
                var saveDialog = new OpenFolderDialog
                {
                    Title = "选择Locale Emulator保存位置"
                };

                // 获取主窗口
                if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    LoggerHelper.LogInformation("[LE安装] 获取主窗口成功");
                    var selectedPath = await saveDialog.ShowAsync(desktop.MainWindow);
                    LoggerHelper.LogInformation($"[LE安装] 用户选择的路径: {selectedPath ?? "null"}");
                    
                    if (!string.IsNullOrEmpty(selectedPath))
                    {
                        // 复制文件到用户选择的位置
                        string targetPath = Path.Combine(selectedPath, "Locale Emulator");
                        LoggerHelper.LogInformation($"[LE安装] 目标路径: {targetPath}");
                        LoggerHelper.LogInformation("[LE安装] 开始复制文件...");
                        CopyDirectory(extractPath, targetPath, true);
                        LoggerHelper.LogInformation("[LE安装] 文件复制完成");
                        
                        // 更新全局路径配置
                        master.Globals.GlobalPaths.TransitSoftwareLE = Path.Combine(targetPath, "LEProc.exe");
                        LoggerHelper.LogInformation($"[LE安装] LE路径配置: {master.Globals.GlobalPaths.TransitSoftwareLE}");
                        
                        // 保存到配置文件
                        LoggerHelper.LogInformation("[LE安装] 保存配置到INI文件...");
                        var iniFile = new Masuit.Tools.Files.IniFile(master.Globals.GlobalPaths.InitiatorProfileName);
                        iniFile.SetValue("main", "TransitSoftwareLE", master.Globals.GlobalPaths.TransitSoftwareLE);
                        iniFile.Save();
                        LoggerHelper.LogInformation("[LE安装] 配置文件保存完成");
                        
                        InstallStatus = $"安装完成！已保存到: {targetPath}";
                        LoggerHelper.LogInformation($"[LE安装] 安装成功: {targetPath}");
                        ShowMessage?.Invoke(this, $"Locale Emulator安装成功！\n已自动配置路径: {master.Globals.GlobalPaths.TransitSoftwareLE}");
                    }
                    else
                    {
                        InstallStatus = "用户取消了保存操作";
                        LoggerHelper.LogInformation("[LE安装] 用户取消了保存操作");
                    }
                }
                else
                {
                    LoggerHelper.LogError("[LE安装] 无法获取主窗口");
                }
            }
            catch (Exception ex)
            {
                InstallStatus = $"安装过程中发生错误: {ex.Message}";
                LoggerHelper.LogError($"[LE安装错误] {ex.Message}");
                LoggerHelper.LogError($"[LE安装错误] 异常类型: {ex.GetType().Name}");
                LoggerHelper.LogError($"[LE安装错误] 堆栈跟踪: {ex.StackTrace}");
                ShowMessage?.Invoke(this, $"安装失败: {ex.Message}");
            }
            finally
            {
                // 清理临时文件
                LoggerHelper.LogInformation("[LE安装] 开始清理临时文件...");
                try
                {
                    string tempPath = Path.GetTempPath();
                    string zipFilePath = Path.Combine(tempPath, "LocaleEmulator.zip");
                    string extractPath = Path.Combine(tempPath, "LocaleEmulator");
                    
                    if (File.Exists(zipFilePath))
                    {
                        LoggerHelper.LogInformation($"[LE安装] 删除ZIP文件: {zipFilePath}");
                        File.Delete(zipFilePath);
                    }
                    if (Directory.Exists(extractPath))
                    {
                        LoggerHelper.LogInformation($"[LE安装] 删除解压目录: {extractPath}");
                        Directory.Delete(extractPath, true);
                    }
                    LoggerHelper.LogInformation("[LE安装] 临时文件清理完成");
                }
                catch (Exception cleanupEx)
                {
                    LoggerHelper.LogError($"[LE安装清理错误] {cleanupEx.Message}");
                }
                
                IsInstalling = false;
                LoggerHelper.LogInformation("=== LE安装流程结束 ===");
            }
        }

        private void CopyDirectory(string sourceDir, string targetDir, bool overwrite)
        {
            LoggerHelper.LogInformation($"[复制目录] 源: {sourceDir}, 目标: {targetDir}");
            Directory.CreateDirectory(targetDir);
            LoggerHelper.LogInformation($"[复制目录] 创建目标目录: {targetDir}");

            // 复制文件
            var files = Directory.GetFiles(sourceDir);
            LoggerHelper.LogInformation($"[复制目录] 找到 {files.Length} 个文件");
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(targetDir, fileName);
                try
                {
                    File.Copy(file, destFile, overwrite);
                    LoggerHelper.LogInformation($"[复制文件] {fileName} -> {destFile}");
                }
                catch (Exception ex)
                {
                    LoggerHelper.LogError($"[复制文件错误] {fileName}: {ex.Message}");
                    throw;
                }
            }

            // 复制子目录
            var directories = Directory.GetDirectories(sourceDir);
            LoggerHelper.LogInformation($"[复制目录] 找到 {directories.Length} 个子目录");
            foreach (string directory in directories)
            {
                string dirName = Path.GetFileName(directory);
                string destDir = Path.Combine(targetDir, dirName);
                LoggerHelper.LogInformation($"[复制子目录] {dirName}");
                CopyDirectory(directory, destDir, overwrite);
            }
            LoggerHelper.LogInformation($"[复制目录] 完成复制: {sourceDir} -> {targetDir}");
        }


        private bool IsLEInstalled()
        {
            // 检查常见的Locale Emulator安装路径
            string[] possiblePaths = {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Locale Emulator"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Locale Emulator"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Locale Emulator")
            };

            foreach (string path in possiblePaths)
            {
                if (Directory.Exists(path) && File.Exists(Path.Combine(path, "LEProc.exe")))
                {
                    return true;
                }
            }

            // 也可以检查注册表
            try
            {
                using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Locale Emulator"))
                {
                    return key != null;
                }
            }
            catch
            {
                return false;
            }
        }

        private void OnBack()
        {
            // 返回逻辑
            LoggerHelper.LogInformation("用户选择返回");
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }
}
