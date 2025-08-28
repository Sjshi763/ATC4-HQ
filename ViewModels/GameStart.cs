using System;
using master.Globals;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using ATC4_HQ.Views;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

namespace ATC4_HQ.ViewModels;

public class GameStart
{
    public async Task StartGame(string selectedGame)
    {
        try
        {
            Console.WriteLine($"准备启动游戏: {selectedGame}");

            // 设置游戏需要的路径
            // if (1==1) // 测试条件
            // 正式条件：
            if (string.IsNullOrEmpty(GlobalPaths.TransitSoftwareLE))
            {
                Console.WriteLine("爷LE呢！？");
                var dialogWindow = new ATC4_HQ.Views.LEInstallWindow();
                var mainWindow = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop 
                    ? desktop.MainWindow 
                    : null;
                bool result = await dialogWindow.ShowDialog(mainWindow);
                if (!result)
                {
                    Console.WriteLine("用户取消了操作，停止启动游戏。");
                    return; // 用户取消操作
                }
                Console.WriteLine("对话框已确认，继续执行");
                Console.WriteLine($"");
                //TODO: 这里可以添加代码来打开设置界面，或者直接当场配置
            }
            string gamePath = GlobalPaths.GamePath + @"\AXA.exe";
            string LEin = GlobalPaths.TransitSoftwareLE;

            // 硬编码路径用于测试
            LEin = @"B:\Locale-Emulator-2.5.0.1\LEProc.exe";
            gamePath = @"A:\ATC4\ATC4" + @"\AXA.exe";
            
            if (LEin == null || gamePath == null)
            {
                Console.WriteLine("错误: LEin 或 gamePath 为空");
                return;
            }

            Console.WriteLine($"使用LE: {LEin}");
            Console.WriteLine($"游戏路径: {gamePath}");

            // 检查文件是否存在
            if (!File.Exists(LEin))
            {
                Console.WriteLine($"错误: 本地模拟器不存在于路径: {LEin}");
                throw new FileNotFoundException("找不到本地LE", LEin);
                // 可选：显示友好的错误提示
                // var dialogWindow = new WarningPop_up();
                // if (dialogWindow.Content is TextBlock contentBlock)
                // {
                //     contentBlock.Text = $"找不到 Locale Emulator，请检查路径：{LEin}";
                // }
                // dialogWindow.ShowDialog(mainWindow);
                // return;
            }

            if (!File.Exists(gamePath))
            {
                Console.WriteLine($"错误: 游戏可执行文件不存在于路径: {gamePath}");
                throw new FileNotFoundException("找不到游戏程序", gamePath);
                // 可选：显示友好的错误提示
                // var dialogWindow = new WarningPop_up();
                // if (dialogWindow.Content is TextBlock contentBlock)
                // {
                //     contentBlock.Text = $"找不到游戏程序，请检查路径：{gamePath}";
                // }
                // dialogWindow.ShowDialog(mainWindow);
                // return;
            }

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = LEin,
                Arguments = gamePath, // 可选改进：Arguments = $"\"{gamePath}\"", // 处理包含空格的路径
                UseShellExecute = true,
                CreateNoWindow = false
            };

            Console.WriteLine("正在启动进程...");
            using (Process? process = Process.Start(psi))
            {
                if (process == null)
                {
                    Console.WriteLine("启动失败：进程为null");
                    throw new Exception("无法启动游戏进程");
                }

                Console.WriteLine($"游戏已成功启动，进程ID: {process.Id}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"启动游戏时发生错误: {ex.Message}");
            Console.WriteLine($"错误详情: {ex.StackTrace}");
            throw;
            // 可选：显示友好的错误提示
            // var dialogWindow = new WarningPop_up();
            // if (dialogWindow.Content is TextBlock contentBlock)
            // {
            //     contentBlock.Text = $"启动游戏时发生错误：{ex.Message}";
            // }
            // dialogWindow.ShowDialog(mainWindow);
        }
    }
}
