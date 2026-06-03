using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace ATC4_HQ.ViewModels;

public class GameStart
{
    public Task StartGame(string selectedGame)
    {
        try
        {
            LoggerHelper.LogInformation($"准备启动游戏: {selectedGame}");

            string gamePath = Path.Combine(selectedGame, "AXA.exe");
            LoggerHelper.LogInformation($"游戏路径: {gamePath}");

            if (!File.Exists(gamePath))
            {
                LoggerHelper.LogError($"错误: 游戏可执行文件不存在于路径: {gamePath}");
                throw new FileNotFoundException("找不到游戏程序", gamePath);
            }

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = gamePath,
                UseShellExecute = true,
                CreateNoWindow = false
            };

            LoggerHelper.LogInformation("正在启动进程...");
            using (Process? process = Process.Start(psi))
            {
                if (process == null)
                {
                    LoggerHelper.LogError("启动失败：进程为null");
                    throw new Exception("无法启动游戏进程");
                }

                LoggerHelper.LogInformation($"游戏已成功启动，进程ID: {process.Id}");
            }

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            LoggerHelper.LogError($"启动游戏时发生错误: {ex.Message}");
            LoggerHelper.LogError($"错误详情: {ex.StackTrace}");
            throw;
        }
    }
}
