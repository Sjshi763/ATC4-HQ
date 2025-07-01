using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.IO.Pipes; // 重新添加：用于命名管道
using System.Text;    // 重新添加：用于文本编码
using System.Threading.Tasks;
using System.Windows.Input;

namespace ATC4_HQ.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ViewModelBase? _currentPage; // 当前显示在 PageHost 中的 ViewModel

        public ICommand StartGameCommand { get; }
        public ICommand InstallGameCommand { get; } // 用于 ViewModel 内部逻辑或未来绑定
        public ICommand SettingCommand { get; }

        public MainWindowViewModel()
        {
            StartGameCommand = new RelayCommand(OnStartGame);
            InstallGameCommand = new RelayCommand(OnInstallGame); 
            SettingCommand = new RelayCommand(OnSetting);

            // 可以在这里设置 PageHost 的初始内容
            // 例如：CurrentPage = new InstallGameViewModel(); 
        }

        private void OnStartGame()
        {
            Console.WriteLine("ViewModel: 启动游戏逻辑。");
            // 实现启动游戏的业务逻辑
        }

        private void OnInstallGame()
        {
            Console.WriteLine("ViewModel: 安装游戏按钮内部逻辑（如果需要）。");
            // 备注：打开对话框的逻辑在 MainWindow.axaml.cs 的 InstallGame_Click 中
            // 这个方法可以用于按钮的内部逻辑，或者当您把 InstallGame_Click 改为 Command 绑定时使用
        }

        private void OnSetting()
        {
            Console.WriteLine("ViewModel: 设置逻辑。");
        }

        // 处理从对话框获取到的路径，并执行 IPC
        public async Task HandleInstallGamePathAndIpcAsync(string path)
        {
            Console.WriteLine($"MainWindowViewModel 接收到路径并准备执行 IPC: {path}");
            await SendPathViaIpcAsync(path); // 调用 IPC 逻辑
        }

        // IPC 逻辑：现在重新放回 MainWindowViewModel 中
        private async Task SendPathViaIpcAsync(string path)
        {
            Console.WriteLine($"尝试通过 IPC 发送路径 (在 ViewModel 中): {path}");
            try
            {
                using var pipeClient = new NamedPipeClientStream(".", "ATC4Pipe", PipeDirection.Out, PipeOptions.Asynchronous);
                await pipeClient.ConnectAsync(5000); 
                
                if (pipeClient.IsConnected)
                {
                    byte[] messageBytes = Encoding.UTF8.GetBytes(path);
                    await pipeClient.WriteAsync(messageBytes, 0, messageBytes.Length);
                    Console.WriteLine("路径已通过 IPC 发送。");
                }
                else
                {
                    Console.WriteLine("IPC 客户端连接失败: 无法连接到命名管道服务器。");
                }
            }
            catch (TimeoutException)
            {
                Console.WriteLine("IPC 连接超时: 无法在指定时间内连接到命名管道服务器。");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"IPC 发送错误: {ex.Message}");
            }
        }
    }
}