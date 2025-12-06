using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Text.Json; // 用于 JSON 序列化
using System.Windows.Input;
using System.Net.Http; // ⭐️ 新增：用于网络请求
using System.Threading.Tasks; // ⭐️ 新增：用于异步编程
using ATC4_HQ.ViewModels; // 添加引用以使用DriveTypeService
using ATC4_HQ.Models; // 引入 GameModel 的命名空间
using System.Collections.Generic; // 用于List
using System.Linq; // 用于LINQ查询

namespace ATC4_HQ.ViewModels
{
    public partial class InstallGameDataViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _gameName = "未命名游戏"; // ⭐️ 新增：绑定到游戏名称输入框

        [ObservableProperty]
        private string _gamePath = "未选择任何文件夹"; // 绑定到 TextBox

        [ObservableProperty]
        private string? _dialogResultData; // ⭐️ 修改：用于存储最终要返回的 JSON 字符串数据

        [ObservableProperty]
        private bool _isDialogOk; // 用于指示对话框是“确定”关闭还是“取消”关闭
        
        [ObservableProperty]
        private bool _shouldClose;

        [ObservableProperty]
        private string _ssdWarning = string.Empty; // 用于显示SSD警告

        private string? _tempDownloadPath; // 用于存储下载过程中的临时文件路径

        // 用于触发 View 执行文件选择操作的事件
        public event EventHandler? RequestOpenFilePicker; // ⭐️ 标记为可为 null 的事件，解决警告
        public event EventHandler<SaveFileDialogEventArgs>? RequestSaveFileDialog; // ⭐️ 新增：用于请求保存文件对话框的事件
        public event EventHandler<InstallGameDataCompletedEventArgs>? InstallGameDataCompleted; // ⭐️ 新增：安装游戏数据完成事件
        public event EventHandler? ClearSubPageRequested; // ⭐️ 新增：请求清除右边区域的事件
        public event EventHandler<BtDownloadEventArgs>? RequestBtDownload; // ⭐️ 新增：请求BT下载的事件


        public ICommand FindFileCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand DownloadCommand { get; } // ⭐️ 新增：下载命令

        public InstallGameDataViewModel()
        {
            FindFileCommand = new RelayCommand(OnFindFile);
            SaveCommand = new RelayCommand(OnSave);
            CancelCommand = new RelayCommand(OnCancel);
            DownloadCommand = new AsyncRelayCommand(OnDownload); // ⭐️ 新增：初始化下载命令
        }

        // --- 命令的实现 ---

        private async Task OnDownload()
        {
            Console.WriteLine("=== 下载流程开始 ===");
            
            // 步骤1: 准备下载（生成临时文件路径）
            Console.WriteLine("[步骤1] 准备下载到临时路径");
            _tempDownloadPath = System.IO.Path.GetTempFileName();
            Console.WriteLine($"[步骤1] 临时文件路径: {_tempDownloadPath}");

            string tempPath = _tempDownloadPath; // 使用类级别变量

            try
            {
                // 步骤2: 准备下载信息
                Console.WriteLine("[步骤2] 准备下载文件信息");
                string fileName = "ATC4ALL.zip";
                string url = $"http://localhost:8080/download?file={fileName}";
                Console.WriteLine($"[步骤2] 下载文件名: {fileName}");
                Console.WriteLine($"[步骤2] 最终下载URL: {url}");

                // 步骤3: 发送HTTP请求（添加超时和重试机制）
                Console.WriteLine("[步骤3] 发送HTTP GET请求...");
                using (var httpClient = new HttpClient())
                {
                    // 设置请求超时
                    httpClient.Timeout = TimeSpan.FromMinutes(10);
                    
                    var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                    Console.WriteLine($"[步骤3] HTTP响应状态码: {response.StatusCode}");
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"[步骤3] HTTP请求失败: {response.StatusCode} - {response.ReasonPhrase}");
                        throw new HttpRequestException($"HTTP请求失败: {response.StatusCode}");
                    }
                    Console.WriteLine("[步骤3] HTTP请求成功");

                    // 步骤4: 获取文件信息
                    Console.WriteLine("[步骤4] 获取文件信息");
                    long? totalBytes = response.Content.Headers.ContentLength;
                    Console.WriteLine($"[步骤4] 文件总大小: {(totalBytes.HasValue ? $"{totalBytes.Value} 字节 ({totalBytes.Value / 1024.0 / 1024.0:F2} MB)" : "未知")}");

                    // 步骤5: 下载并写入临时文件
                    Console.WriteLine("[步骤5] 开始下载并写入临时文件");
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = new System.IO.FileStream(tempPath, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.None))
                    {
                        Console.WriteLine($"[步骤5] 目标临时文件路径: {tempPath}");
                        
                        // 缓冲区大小
                        byte[] buffer = new byte[8192];
                        long totalBytesRead = 0;
                        int bytesRead;
                        int progressPercentage = 0;

                        Console.WriteLine("[步骤5] 开始下载文件内容...");
                        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                            totalBytesRead += bytesRead;
                            
                            // 打印下载进度（每5%更新一次，避免日志过多）
                            if (totalBytes.HasValue)
                            {
                                int newProgress = (int)((double)totalBytesRead / totalBytes.Value * 100);
                                if (newProgress >= progressPercentage + 5 || newProgress == 100)
                                {
                                    progressPercentage = newProgress;
                                    Console.WriteLine($"[步骤5] 下载进度: {progressPercentage}% ({totalBytesRead}/{totalBytes.Value} 字节)");
                                }
                            }
                            else
                            {
                                // 每下载1MB打印一次进度
                                if (totalBytesRead % (1024 * 1024) == 0)
                                {
                                    Console.WriteLine($"[步骤5] 已下载: {totalBytesRead} 字节 ({totalBytesRead / 1024.0 / 1024.0:F2} MB)");
                                }
                            }
                        }
                        
                        Console.WriteLine($"[步骤5] 文件下载完成，总字节数: {totalBytesRead} ({totalBytesRead / 1024.0 / 1024.0:F2} MB)");
                    }

                    // 步骤6: 下载完成 - 提示用户选择保存位置
                    Console.WriteLine("[步骤6] 下载流程完成");
                    
                    // 触发保存文件对话框，让用户选择保存位置
                    string? finalPath = await ShowSaveFileDialog(tempPath);
                    if (!string.IsNullOrEmpty(finalPath))
                    {
                        GamePath = finalPath; 
                        Console.WriteLine($"[步骤6] 文件已保存到用户指定路径: {finalPath}");
                    }
                    else
                    {
                        // 如果用户取消了保存对话框，仍然使用临时路径
                        GamePath = tempPath; 
                        Console.WriteLine($"[步骤6] 用户取消保存，文件保留在临时路径: {tempPath}");
                    }
                    Console.WriteLine("=== 下载流程结束 ===");
                }
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"[网络错误] 下载失败: {httpEx.Message}");
                Console.WriteLine($"[网络错误] 请检查网络连接或服务器状态");
                Console.WriteLine("=== 下载流程结束（失败） ===");
                
                // 清理临时文件
                try
                {
                    if (System.IO.File.Exists(tempPath))
                    {
                        System.IO.File.Delete(tempPath);
                        Console.WriteLine("[清理] 已删除临时文件");
                    }
                }
                catch (Exception cleanupEx)
                {
                    Console.WriteLine($"[清理错误] 删除临时文件失败: {cleanupEx.Message}");
                }
            }
            catch (TaskCanceledException tcEx) when (tcEx.InnerException is System.TimeoutException)
            {
                Console.WriteLine($"[超时错误] 下载超时: 请求超时，请检查网络连接");
                Console.WriteLine("=== 下载流程结束（失败） ===");
                
                // 清理临时文件
                try
                {
                    if (System.IO.File.Exists(tempPath))
                    {
                        System.IO.File.Delete(tempPath);
                        Console.WriteLine("[清理] 已删除临时文件");
                    }
                }
                catch (Exception cleanupEx)
                {
                    Console.WriteLine($"[清理错误] 删除临时文件失败: {cleanupEx.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[错误] 下载失败: {ex.Message}");
                Console.WriteLine($"[错误] 异常类型: {ex.GetType().Name}");
                Console.WriteLine("=== 下载流程结束（失败） ===");
                
                // 清理临时文件
                try
                {
                    if (System.IO.File.Exists(tempPath))
                    {
                        System.IO.File.Delete(tempPath);
                        Console.WriteLine("[清理] 已删除临时文件");
                    }
                }
                catch (Exception cleanupEx)
                {
                    Console.WriteLine($"[清理错误] 删除临时文件失败: {cleanupEx.Message}");
                }
            }
        }

        private void OnFindFile()
        {
            // ViewModel 触发事件，告知 View 去执行文件选择器
            RequestOpenFilePicker?.Invoke(this, EventArgs.Empty);
            // View 会处理 RequestOpenFilePicker 事件，然后将选定的路径赋值给 GamePath 属性
        }

        partial void OnGamePathChanged(string value)
        {
            // 检测路径是否在SSD上并更新警告
            if (!string.IsNullOrWhiteSpace(value) && value != "未选择任何文件夹")
            {
                if (DriveTypeService.IsDriveSSD(value) && DriveTypeService.HasHDD())
                {
                    SsdWarning = "提示：检测到您正在使用SSD安装游戏，普通HDD就足够流畅运行！";
                }
                else
                {
                    SsdWarning = string.Empty;
                }
            }
            else
            {
                SsdWarning = string.Empty;
            }
        }

        private async void OnSave()
        {
            // ⭐️ 新增：对 GameName 的验证
            if (string.IsNullOrWhiteSpace(GameName) || GameName == "取个名字方便找到它")
            {
                Console.WriteLine("请为游戏输入一个名称！");
                // 可以在 UI 上显示错误提示
                return;
            }

            // 在这里可以添加验证逻辑，确保 GamePath 是有效的
            if (string.IsNullOrWhiteSpace(GamePath) || GamePath == "未选择任何文件夹")
            {
                Console.WriteLine("请选择一个有效的游戏路径！");
                return;
            }
            
            // 即使检测到SSD也允许安装，只是显示警告
            // 这里不阻止安装，只显示信息性提示

            // ⭐️ 新增：检查是否有BT下载链接
            var btLink = await GetBtDownloadLinkAsync(GameName);
            if (!string.IsNullOrEmpty(btLink))
            {
                Console.WriteLine($"发现游戏 {GameName} 的BT下载链接");
                
                // 触发BT下载事件
                var btArgs = new BtDownloadEventArgs
                {
                    GameName = GameName,
                    MagnetLink = btLink,
                    DownloadPath = GamePath
                };
                
                RequestBtDownload?.Invoke(this, btArgs);
            }

            // ⭐️ 修改：创建 GameModel 对象并触发完成事件
            var gameData = new GameModel 
            {
                Name = GameName,
                Path = GamePath
            };

            // 触发完成事件
            InstallGameDataCompleted?.Invoke(this, new InstallGameDataCompletedEventArgs(true, gameData));
        }

        private void OnCancel()
        {
            // 删除临时下载文件
            try
            {
                if (!string.IsNullOrEmpty(_tempDownloadPath) && System.IO.File.Exists(_tempDownloadPath))
                {
                    System.IO.File.Delete(_tempDownloadPath);
                    Console.WriteLine("[取消] 已删除临时下载文件");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[取消错误] 删除临时文件失败: {ex.Message}");
            }

            // 触发清除右边区域的事件
            ClearSubPageRequested?.Invoke(this, EventArgs.Empty);
            
            // 触发完成事件（失败）
            InstallGameDataCompleted?.Invoke(this, new InstallGameDataCompletedEventArgs(false, null));
        }

        // 显示保存文件对话框，让用户选择文件保存位置
        private async Task<string?> ShowSaveFileDialog(string tempFilePath)
        {
            var args = new SaveFileDialogEventArgs();
            
            // 触发保存文件对话框事件
            RequestSaveFileDialog?.Invoke(this, args);
            
            // 等待用户选择结果
            string? selectedPath = await args.GetResultAsync();
            
            if (!string.IsNullOrEmpty(selectedPath))
            {
                try
                {
                    // 将临时文件移动到用户选择的位置
                    if (System.IO.File.Exists(tempFilePath))
                    {
                        // 如果目标文件已存在，先删除
                        if (System.IO.File.Exists(selectedPath))
                        {
                            System.IO.File.Delete(selectedPath);
                        }
                        
                        // 移动文件
                        System.IO.File.Move(tempFilePath, selectedPath);
                        Console.WriteLine($"[保存] 文件已移动到: {selectedPath}");
                        return selectedPath;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[保存错误] 移动文件失败: {ex.Message}");
                }
            }
            
            return selectedPath;
        }

        // ⭐️ 新增：获取游戏的BT下载链接
        private async Task<string?> GetBtDownloadLinkAsync(string gameName)
        {
            try
            {
                // 读取files-link.json文件
                string jsonFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ATC4-HQ-DATA", "install-files", "files-link.json");
                
                if (System.IO.File.Exists(jsonFilePath))
                {
                    string jsonContent = await System.IO.File.ReadAllTextAsync(jsonFilePath);
                    
                    // 解析JSON
                    using (JsonDocument doc = JsonDocument.Parse(jsonContent))
                    {
                        JsonElement root = doc.RootElement;
                        
                        // 获取main-games数组
                        if (root.TryGetProperty("main-games", out JsonElement mainGames))
                        {
                            // 遍历游戏列表
                            foreach (JsonElement game in mainGames.EnumerateArray())
                            {
                                if (game.TryGetProperty("name", out JsonElement nameElement) &&
                                    game.TryGetProperty("url", out JsonElement urlElement))
                                {
                                    string gameNameInJson = nameElement.GetString() ?? "";
                                    string url = urlElement.GetString() ?? "";
                                    
                                    // 检查游戏名称是否匹配（不区分大小写）
                                    if (string.Equals(gameNameInJson, gameName, StringComparison.OrdinalIgnoreCase))
                                    {
                                        Console.WriteLine($"找到游戏 {gameName} 的BT链接: {url}");
                                        return url;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"BT链接文件不存在: {jsonFilePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"读取BT链接文件失败: {ex.Message}");
            }
            
            return null;
        }
    }

    // ⭐️ 新增：用于传递保存文件对话框参数的事件参数类
    public class SaveFileDialogEventArgs : EventArgs
    {
        private readonly TaskCompletionSource<string?> _tcs = new TaskCompletionSource<string?>();

        public void SetResult(string? result)
        {
            _tcs.SetResult(result);
        }

        public Task<string?> GetResultAsync()
        {
            return _tcs.Task;
        }
    }

    // ⭐️ 新增：用于传递安装游戏数据完成事件参数的类
    public class InstallGameDataCompletedEventArgs : EventArgs
    {
        public bool Success { get; }
        public GameModel? GameData { get; }

        public InstallGameDataCompletedEventArgs(bool success, GameModel? gameData)
        {
            Success = success;
            GameData = gameData;
        }
    }

    // ⭐️ 新增：用于传递BT下载事件参数的类
    public class BtDownloadEventArgs : EventArgs
    {
        public string GameName { get; set; } = string.Empty;
        public string MagnetLink { get; set; } = string.Empty;
        public string DownloadPath { get; set; } = string.Empty;
    }
}
