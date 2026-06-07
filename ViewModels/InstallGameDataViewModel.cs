using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Text.Json; // 用于 JSON 序列化
using System.Windows.Input;
using System.Net.Http; // ⭐️ 新增：用于网络请求
using System.Threading.Tasks; // ⭐️ 新增：用于异步编程
using ATC4_HQ.ViewModels; // 添加引用以使用DriveTypeService
using System.Collections.Generic; // 用于List
using System.Linq; // 用于LINQ查询
using System.IO;
using master.Globals;

namespace ATC4_HQ.ViewModels
{
    public partial class InstallGameDataViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _gameName = "未命名游戏"; // ⭐️ 新增：绑定到游戏名称输入框

        [ObservableProperty]
        private string _gamePath = "未选择任何文件夹"; // 压缩包文件夹路径

        [ObservableProperty]
        private string _installPath = string.Empty; // 游戏安装路径

        [ObservableProperty]
        private string? _dialogResultData; // ⭐️ 修改：用于存储最终要返回的 JSON 字符串数据

        [ObservableProperty]
        private bool _isDialogOk; // 用于指示对话框是“确定”关闭还是“取消”关闭
        
        [ObservableProperty]
        private bool _shouldClose;

        [ObservableProperty]
        private string _ssdWarning = string.Empty; // 用于显示SSD警告

        [ObservableProperty]
        private string _archiveValidationMessage = "请选择包含 ATC4.z01 到 ATC4.z08 以及 ATC4.zip 的完整文件夹。";

        [ObservableProperty]
        private bool _isArchiveFolderValid;

        public string RequiredArchivePartsText => string.Join("、", GlobalPaths.RequiredAtc4ArchiveParts);

        // 用于触发 View 执行文件选择操作的事件
        public event EventHandler? RequestOpenFilePicker; // ⭐️ 标记为可为 null 的事件，解决警告
        public event EventHandler? RequestOpenFolderPicker; // ⭐️ 新增：用于请求打开文件夹选择器的事件
        public event EventHandler<SaveFileDialogEventArgs>? RequestSaveFileDialog; // ⭐️ 新增：用于请求保存文件对话框的事件
        public event EventHandler<InstallGameDataCompletedEventArgs>? InstallGameDataCompleted; // ⭐️ 新增：安装游戏数据完成事件
        public event EventHandler? ClearSubPageRequested; // ⭐️ 新增：请求清除右边区域的事件


        public ICommand FindFileCommand { get; }
        public ICommand BrowseInstallPathCommand { get; }
        public ICommand SaveCommand { get; }

        public InstallGameDataViewModel()
        {
            FindFileCommand = new RelayCommand(OnFindFile);
            BrowseInstallPathCommand = new RelayCommand(OnBrowseInstallPath);
            SaveCommand = new RelayCommand(OnSave);
        }

        // --- 命令的实现 ---



        private void OnFindFile()
        {
            // ViewModel 触发事件，告知 View 去执行文件选择器
            RequestOpenFilePicker?.Invoke(this, EventArgs.Empty);
            // View 会处理 RequestOpenFilePicker 事件，然后将选定的路径赋值给 GamePath 属性
        }

        private void OnBrowseInstallPath()
        {
            // ViewModel 触发事件，告知 View 去执行文件夹选择器
            RequestOpenFolderPicker?.Invoke(this, EventArgs.Empty);
            // View 会处理 RequestOpenFolderPicker 事件，然后将选定的路径赋值给 InstallPath 属性
        }

        partial void OnGamePathChanged(string value)
        {
            ValidateArchiveFolder(value);

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

        private void ValidateArchiveFolder(string? folderPath)
        {
            IsArchiveFolderValid = false;

            if (string.IsNullOrWhiteSpace(folderPath) || folderPath == "未选择任何文件夹")
            {
                ArchiveValidationMessage = "请选择包含 ATC4.z01 到 ATC4.z08 以及 ATC4.zip 的完整文件夹。";
                return;
            }

            if (!Directory.Exists(folderPath))
            {
                ArchiveValidationMessage = "所选路径不是有效文件夹，请重新选择。";
                return;
            }

            var missingParts = GlobalPaths.RequiredAtc4ArchiveParts
                .Where(part => !File.Exists(Path.Combine(folderPath, part)))
                .ToArray();

            if (missingParts.Length > 0)
            {
                ArchiveValidationMessage = $"文件夹缺少必要分卷：{string.Join("、", missingParts)}";
                return;
            }

            IsArchiveFolderValid = true;
            ArchiveValidationMessage = "已检测到完整 ATC4 分卷压缩包，可以开始安装。";
        }

        private void OnSave()
        {
            LoggerHelper.LogInformation($"[用户点击安装] 准备安装游戏: {GameName} 喵");

            // ⭐️ 新增：对 GameName 的验证
            if (string.IsNullOrWhiteSpace(GameName) || GameName == "取个名字方便找到它")
            {
                LoggerHelper.LogError("[安装失败] 游戏名称为空或未修改 喵");
                return;
            }

            // 验证压缩包路径
            if (string.IsNullOrWhiteSpace(GamePath) || GamePath == "未选择任何文件夹")
            {
                LoggerHelper.LogError("[安装失败] 未选择有效的压缩包文件夹 喵");
                return;
            }

            // 验证安装路径
            if (string.IsNullOrWhiteSpace(InstallPath))
            {
                LoggerHelper.LogError("[安装失败] 未选择有效的游戏安装路径 喵");
                return;
            }

            LoggerHelper.LogInformation("[安装校验] 正在验证压缩包分卷完整性... 喵");
            ValidateArchiveFolder(GamePath);
            if (!IsArchiveFolderValid)
            {
                LoggerHelper.LogError($"[安装失败] 压缩包校验未通过: {ArchiveValidationMessage} 喵");
                return;
            }

            LoggerHelper.LogInformation("[安装校验] 校验成功，正在准备游戏数据模型... 喵");
            
            // 即使检测到SSD也允许安装，只是显示警告
            // 这里不阻止安装，只显示信息性提示

            // ⭐️ 修改：创建 GameModel 对象并触发完成事件
            var gameData = new GameModel 
            {
                Name = GameName,
                Path = InstallPath,
                ArchivePath = GamePath
            };

            LoggerHelper.LogInformation($"[安装成功] 已成功记录安装信息：{GameName} -> {InstallPath} (压缩包: {GamePath}) 喵");

            // 触发完成事件
            InstallGameDataCompleted?.Invoke(this, new InstallGameDataCompletedEventArgs(true, gameData));
        }

        private void OnCancel()
        {
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
                        LoggerHelper.LogInformation($"[保存] 文件已移动到: {selectedPath}");
                        return selectedPath;
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.LogError($"[保存错误] 移动文件失败: {ex.Message}");
                }
            }
            
            return selectedPath;
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

}
