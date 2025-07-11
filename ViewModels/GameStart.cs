using System;
using master.Globals;
using System.Diagnostics;
using System.IO;

namespace ATC4_HQ.ViewModels;

public class GameStart
{
    public void StartGame(string selectedGame)
    {
        try
        {
            Console.WriteLine($"准备启动游戏: {selectedGame}");

            // 设置游戏路径
            string gamePath = GlobalPaths.GamePath + @"\AXA.exe";
            string LEin = GlobalPaths.TransitSoftwareLE;

            // 硬编码路径用于测试
            LEin = @"B:\Locale-Emulator-2.5.0.1\LEProc.exe";
            gamePath = @"A:\ATC4\ATC4" + @"\AXA.exe";

            Console.WriteLine($"使用LE: {LEin}");
            Console.WriteLine($"游戏路径: {gamePath}");

            // 检查文件是否存在
            if (!File.Exists(LEin))
            {
                Console.WriteLine($"错误: 本地模拟器不存在于路径: {LEin}");
                throw new FileNotFoundException("找不到本地模拟器程序", LEin);
            }

            if (!File.Exists(gamePath))
            {
                Console.WriteLine($"错误: 游戏可执行文件不存在于路径: {gamePath}");
                throw new FileNotFoundException("找不到游戏程序", gamePath);
            }

            // 创建进程启动信息
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = LEin,
                Arguments = gamePath,
                UseShellExecute = true, // 使用系统外壳程序启动
                CreateNoWindow = false  // 创建窗口
            };

            // 实际启动进程
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
            throw; // 重新抛出异常以便调用者处理
        }
    }
}