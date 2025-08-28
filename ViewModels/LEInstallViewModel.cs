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

        public LEInstallViewModel()
        {
            DownloadAndInstallCommand = new RelayCommand(async () => await DownloadAndInstallLEAsync());
            TestInstallCommand = new RelayCommand(TestInstall);
            BackCommand = new RelayCommand(OnBack);
        }

        private async Task DownloadAndInstallLEAsync()
        {
            if (IsInstalling) return;

            IsInstalling = true;
            InstallStatus = "开始下载Locale Emulator最新版本...";

            try
            {
                // GitHub仓库信息
                string repoOwner = "xupefei";
                string repoName = "Locale-Emulator";
                string downloadUrl = $"https://github.com/{repoOwner}/{repoName}/releases/latest/download/Locale.Emulator.2.5.0.1.zip";
                
                // 获取临时目录
                string tempPath = Path.GetTempPath();
                string zipFilePath = Path.Combine(tempPath, "LocaleEmulator.zip");
                string extractPath = Path.Combine(tempPath, "LocaleEmulator");

                // 下载文件
                InstallStatus = "正在下载...";
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(10); // 设置10分钟超时
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

                // 让用户选择保存位置
                var saveDialog = new OpenFolderDialog
                {
                    Title = "选择Locale Emulator保存位置"
                };

                // 获取主窗口
                if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    var selectedPath = await saveDialog.ShowAsync(desktop.MainWindow);
                    if (!string.IsNullOrEmpty(selectedPath))
                    {
                        // 复制文件到用户选择的位置
                        string targetPath = Path.Combine(selectedPath, "Locale Emulator");
                        CopyDirectory(extractPath, targetPath, true);
                        
                        // 更新全局路径配置
                        master.Globals.GlobalPaths.TransitSoftwareLE = Path.Combine(targetPath, "LEProc.exe");
                        
                        // 保存到配置文件
                        var iniFile = new Masuit.Tools.Files.IniFile(master.Globals.GlobalPaths.InitiatorProfileName);
                        iniFile.SetValue("main", "TransitSoftwareLE", master.Globals.GlobalPaths.TransitSoftwareLE);
                        iniFile.Save();
                        
                        InstallStatus = $"安装完成！已保存到: {targetPath}";
                        ShowMessage?.Invoke(this, $"Locale Emulator安装成功！\n已自动配置路径: {master.Globals.GlobalPaths.TransitSoftwareLE}");
                    }
                    else
                    {
                        InstallStatus = "用户取消了保存操作";
                    }
                }
            }
            catch (Exception ex)
            {
                InstallStatus = $"安装过程中发生错误: {ex.Message}";
                ShowMessage?.Invoke(this, $"安装失败: {ex.Message}");
            }
            finally
            {
                // 清理临时文件
                try
                {
                    string tempPath = Path.GetTempPath();
                    string zipFilePath = Path.Combine(tempPath, "LocaleEmulator.zip");
                    string extractPath = Path.Combine(tempPath, "LocaleEmulator");
                    
                    if (File.Exists(zipFilePath))
                        File.Delete(zipFilePath);
                    if (Directory.Exists(extractPath))
                        Directory.Delete(extractPath, true);
                }
                catch (Exception cleanupEx)
                {
                    Console.WriteLine($"清理临时文件时发生错误: {cleanupEx.Message}");
                }
                
                IsInstalling = false;
            }
        }

        private void CopyDirectory(string sourceDir, string targetDir, bool overwrite)
        {
            Directory.CreateDirectory(targetDir);

            // 复制文件
            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(targetDir, fileName);
                File.Copy(file, destFile, overwrite);
            }

            // 复制子目录
            foreach (string directory in Directory.GetDirectories(sourceDir))
            {
                string dirName = Path.GetFileName(directory);
                string destDir = Path.Combine(targetDir, dirName);
                CopyDirectory(directory, destDir, overwrite);
            }
        }

        private void TestInstall()
        {
            // 测试安装逻辑
            System.Console.WriteLine("用户点击测试安装按钮");
            
            // 检查Locale Emulator是否已安装（检查注册表或安装目录）
            if (IsLEInstalled())
            {
                // 显示安装成功的消息
                ShowMessage?.Invoke(this, "测试成功\nLocale Emulator已正确安装！");
            }
            else
            {
                // 显示安装失败的消息
                ShowMessage?.Invoke(this, "测试失败\nLocale Emulator未安装或安装不完整！");
            }
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
            System.Console.WriteLine("用户选择返回");
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }
}
