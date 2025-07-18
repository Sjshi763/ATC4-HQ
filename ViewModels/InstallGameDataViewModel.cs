using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Text.Json; // 用于 JSON 序列化
using System.Windows.Input;

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

        // 用于触发 View 执行文件选择操作的事件
        public event EventHandler? RequestOpenFilePicker; // ⭐️ 标记为可为 null 的事件，解决警告

        public ICommand FindFileCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public InstallGameDataViewModel()
        {
            FindFileCommand = new RelayCommand(OnFindFile);
            SaveCommand = new RelayCommand(OnSave);
            CancelCommand = new RelayCommand(OnCancel);
        }

        // --- 命令的实现 ---

        private void OnFindFile()
        {
            // ViewModel 触发事件，告知 View 去执行文件选择器
            RequestOpenFilePicker?.Invoke(this, EventArgs.Empty);
            // View 会处理 RequestOpenFilePicker 事件，然后将选定的路径赋值给 GamePath 属性
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
}