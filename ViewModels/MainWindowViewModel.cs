using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.IO.Pipes; // 用于命名管道
using System.Text;     // 用于文本编码
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
            // ⭐️ 将 CurrentPage 设置为 GameStartOptionsViewModel 的实例
            CurrentPage = new GameStartOptionsViewModel();
            Console.WriteLine("ViewModel: 已切换到游戏启动选项界面。");
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
            // ⭐️ 调用新的通用 IPC 命令发送方法
            // 现在发送的消息会是 "UNZIP C:\Your\Path\"
            await SendIpcCommandAsync("UNZIP", path); 
        }

        // ⭐️ 新增：一个通用的公共方法，用于发送带命令和参数的 IPC 消息
        // 其他功能现在可以调用这个方法来发送不同的命令
        public async Task SendIpcCommandAsync(string command, string parameter)
        {
            // 构造完整的消息字符串，命令和参数之间用空格分隔
            string fullMessage = $"{command} {parameter}";
            await SendRawMessageViaIpcAsync(fullMessage);
        }

        // ⭐️ 重命名并修改为私有：这是实际进行命名管道通信的底层方法
        // 它现在只负责发送原始消息字节，不再关心消息的格式
        private async Task SendRawMessageViaIpcAsync(string message)
        {
            Console.WriteLine($"尝试通过 IPC 发送原始消息 (在 ViewModel 中): {message}");
            try
            {
                using var pipeClient = new NamedPipeClientStream(".", "ATC4Pipe", PipeDirection.Out, PipeOptions.Asynchronous);
                await pipeClient.ConnectAsync(5000); // 连接超时时间设置为 5 秒
                
                if (pipeClient.IsConnected)
                {
                    // 使用 UTF8 编码发送完整消息（包括命令和参数）
                    byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                    await pipeClient.WriteAsync(messageBytes, 0, messageBytes.Length);
                    Console.WriteLine("原始消息已通过 IPC 发送。");
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

    // ⭐️ 这是 GameStartOptionsViewModel 的定义，它必须在 ATC4_HQ.ViewModels 命名空间内
    // 确保它存在于您的项目中的某个 .cs 文件里，例如 MainWindowViewModel.cs 或者单独的 GameStartOptionsViewModel.cs
    public partial class GameStartOptionsViewModel : ViewModelBase
    {
        public ICommand Button1Command { get; }
        public ICommand Button2Command { get; }

        public GameStartOptionsViewModel()
        {
            Button1Command = new RelayCommand(OnButton1);
            Button2Command = new RelayCommand(OnButton2);
        }

        private void OnButton1()
        {
            Console.WriteLine("Game Start Options: 第一个按钮被点击了！");
            // TODO: 在这里实现上一次游戏的逻辑，例如启动游戏的不同模式
        }

        private void OnButton2()
        {
            Console.WriteLine("Game Start Options: 第二个按钮被点击了！");
            // TODO: 在这里实现全部游戏的逻辑，例如打开游戏设置或另一个功能
        }
    }
}