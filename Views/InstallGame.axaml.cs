using Avalonia.Controls;
using System;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Interactivity; // 确保引用
using ATC4_HQ.ViewModels; // 引用您的 ViewModel 命名空间

namespace ATC4_HQ.Views
{
    public partial class InstallGame : UserControl
    {
        public InstallGame()
        {
            InitializeComponent();
            // 在这里将 ViewModel 设置为 DataContext
            // DataContext = new InstallGameViewModel(); // 假设您有 InstallGameViewModel
            // 或者，如果 InstallGame 本身就是作为某个更大 ViewModel 的 View，则不需要在这里设置
            // 为了简洁，我们假设 MainViewModel 会管理它，或者我们将直接在 OnLoaded 中处理

            // 这是一个 UserControl，通常它的 DataContext 会由其父级设置。
            // 为了确保 MVVM 绑定生效，我们假设您会在父级 View/ViewModel 中设置 InstallGame 的 DataContext。
            // 或者，如果您希望 InstallGame 自身管理其 ViewModel，可以在 Loaded 事件中进行设置
            this.Loaded += InstallGame_Loaded;
            this.Unloaded += InstallGame_Unloaded;
        }

        private void InstallGame_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is InstallGameViewModel viewModel)
            {
                // 订阅 ViewModel 的命令，以处理实际的 UI 交互，例如打开对话框
                // 如果 InstallGame_Click 直接绑定到命令，这里不需要订阅 Click 事件
                // 而是订阅一个 ViewModel 内部的“请求打开对话框”的事件
                viewModel.RequestOpenInstallGameDataDialog += OnRequestOpenInstallGameDataDialog;
            }
        }

        private void InstallGame_Unloaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is InstallGameViewModel viewModel)
            {
                viewModel.RequestOpenInstallGameDataDialog -= OnRequestOpenInstallGameDataDialog;
            }
        }


        // 这个方法将响应 InstallGameViewModel 中触发的事件，用于打开对话框
        private async void OnRequestOpenInstallGameDataDialog(object? sender, EventArgs e)
        {
            var dialogWindow = new InstallGameDataDialogWindow();
            // ShowDialog() 返回一个 bool?，表示对话框的 Result 属性
            // 或者您可以直接通过 dialogWindow.DataContext 来获取 ViewModel 中的 DialogResultPath
            bool? dialogResult = await dialogWindow.ShowDialog<bool?>(TopLevel.GetTopLevel(this) as Window);

            if (dialogResult == true && dialogWindow.DataContext is InstallGameDataViewModel dialogViewModel)
            {
                string? selectedPath = dialogViewModel.DialogResultPath;
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    Console.WriteLine($"从对话框中获取到的路径: {selectedPath}");

                    // 现在，您可以将这个路径传递给 InstallGameViewModel，
                    // 让它负责进行 IPC 通信
                    if (DataContext is InstallGameViewModel currentViewModel)
                    {
                        await currentViewModel.SendPathViaIpcAsync(selectedPath);
                    }
                }
            }
            else
            {
                Console.WriteLine("用户取消了选择或对话框关闭。");
            }
        }

        // 移除原有的 InstallGame_Click 方法，因为它现在由 ViewModel 的命令和上述事件处理
        // 移除 SendPipeMessageAsync 方法，因为 IPC 逻辑将移动到 ViewModel
        // private void InstallGame_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) { ... }
        // private async Task SendPipeMessageAsync(string msg) { ... }
    }
}