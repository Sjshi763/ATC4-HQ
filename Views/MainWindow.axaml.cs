using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Threading.Tasks;
using ATC4_HQ.ViewModels;
using System; // 确保引用此命名空间

// 移除：using System.IO.Pipes; // 不再在这里处理 IPC
// 移除：using System.Text;    // 不再在这里处理 IPC

namespace ATC4_HQ.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // 在这里设置 MainWindow 的 DataContext
            // 确保 App.axaml.cs 中也设置了 DataContext = new MainWindowViewModel();
            // 或者在这里显式设置：
            // DataContext = new MainWindowViewModel(); 

            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            // 如果需要初始化 PageHost 的内容，可以在这里进行
            // 但更好的做法是让 ViewModel 的 CurrentPage 属性来控制
        }

        private void StartGame_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                Console.WriteLine("启动游戏按钮被点击了！");
                viewModel.StartGameCommand.Execute(null); 
            }
        }

        // 安装游戏按钮的点击事件：打开对话框，将结果传递给 ViewModel
        private async void InstallGame_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel) // 确保 ViewModel 已设置
            {
                Console.WriteLine("安装游戏按钮被点击了，准备打开对话框。");

                var dialogWindow = new InstallGameDataDialogWindow();
                Window? ownerWindow = TopLevel.GetTopLevel(this) as Window;
                bool? dialogResult = await dialogWindow.ShowDialog<bool?>(ownerWindow);

                if (dialogResult == true && dialogWindow.DataContext is InstallGameDataViewModel dialogViewModel)
                {
                    string? selectedPath = dialogViewModel.DialogResultPath;
                    if (!string.IsNullOrEmpty(selectedPath))
                    {
                        Console.WriteLine($"从对话框中获取到的路径: {selectedPath}");
                        // 将路径传递给 MainWindowViewModel 来处理 IPC
                        await viewModel.HandleInstallGamePathAndIpcAsync(selectedPath); 
                    }
                }
                else
                {
                    Console.WriteLine("用户取消了选择或对话框关闭。");
                }
            }
        }

        private void Setting_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                Console.WriteLine("设置按钮被点击了！");
                viewModel.SettingCommand.Execute(null);
            }
        }
    }
}