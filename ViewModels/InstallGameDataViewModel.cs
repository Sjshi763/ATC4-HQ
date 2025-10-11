using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Text.Json; // 用于 JSON 序列化
using System.Windows.Input;
using System.Net.Http; // ⭐️ 新增：用于网络请求
using System.Threading.Tasks; // ⭐️ 新增：用于异步编程
using ATC4_HQ.ViewModels; // 添加引用以使用DriveTypeService

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

        // 用于触发 View 执行文件选择操作的事件
        public event EventHandler? RequestOpenFilePicker; // ⭐️ 标记为可为 null 的事件，解决警告
        public event EventHandler<SaveFileDialogEventArgs>? RequestSaveFileDialog; // ⭐️ 新增：用于请求保存文件对话框的事件


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
            var args = new SaveFileDialogEventArgs();
            RequestSaveFileDialog?.Invoke(this, args);

            string? savePath = await args.GetResultAsync();

            if (!string.IsNullOrWhiteSpace(savePath))
            {
                // 在这里执行下载逻辑
                using (var httpClient = new HttpClient())
                {
                    try
                    {
                        // 假设我们要下载的文件名为 "ATC4_XJATC.zip"
                        string fileName = "ATC4_XJATC.zip";
                        string url = $"http://localhost:8080/download?file={fileName}";

                        var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                        response.EnsureSuccessStatusCode();

                        using (var stream = await response.Content.ReadAsStreamAsync())
                        using (var fileStream = new System.IO.FileStream(savePath, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.None))
                        {
                            await stream.CopyToAsync(fileStream);
                        }

                        // 下载完成后，可以将路径设置为下载的文件的路径或解压后的文件夹路径
                        // 这里我们先简单地设置为保存的文件路径
                        GamePath = savePath; 
                        Console.WriteLine($"文件已下载到: {savePath}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"下载失败: {ex.Message}");
                        // 可以在UI上显示错误提示
                    }
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

        private void OnSave()
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

            // ⭐️ 修改：创建 GameModel 对象并序列化为 JSON 字符串
            var gameData = new Models.GameModel // 明确指定命名空间和类名
            {
                Name = GameName,
                Path = GamePath
            };

            // 将 GameModel 对象序列化为 JSON 字符串，并赋值给 DialogResultData
            DialogResultData = JsonSerializer.Serialize(gameData);
            IsDialogOk = true; // 设置对话框结果为 OK
            ShouldClose = true;
        }

        private void OnCancel()
        {
            DialogResultData = null; // 清空结果
            IsDialogOk = false;      // 设置对话框结果为 Cancel
            ShouldClose = true;
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
}
