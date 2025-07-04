using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks; // 尽管文件选择器不在 ViewModel 中，但命令仍可能是异步的
using System.Windows.Input;

namespace ATC4_HQ.ViewModels
{
    public partial class InstallGameDataViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _gamePath = "未选择任何文件夹"; // 绑定到 TextBox

        [ObservableProperty]
        private string? _dialogResultPath; // 用于存储最终要返回的路径

        [ObservableProperty]
        private bool _isDialogOk; // 用于指示对话框是“确定”关闭还是“取消”关闭

        // 用于触发 View 执行文件选择操作的事件
        // View 的代码后台将订阅此事件
        public event EventHandler RequestOpenFilePicker;

        public ICommand FindFileCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public InstallGameDataViewModel()
        {
            FindFileCommand = new RelayCommand(OnFindFile); // 改为 RelayCommand，因为实际文件选择器在 View
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
            // 在这里可以添加验证逻辑，确保 GamePath 是有效的
            if (string.IsNullOrWhiteSpace(GamePath) || GamePath == "未选择任何文件夹")
            {
                // 可以显示一个错误消息或提示用户
                Console.WriteLine("请选择一个有效的游戏路径！");
                return;
            }

            DialogResultPath = GamePath; // 设置要返回的结果
            IsDialogOk = true;           // 设置对话框结果为 OK
        }

        private void OnCancel()
        {
            DialogResultPath = null;    // 清空结果
            IsDialogOk = false;         // 设置对话框结果为 Cancel
        }
    }
}