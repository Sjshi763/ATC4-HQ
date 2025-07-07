using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.IO.Pipes; // 用于命名管道
using System.Text;     // 用于文本编码
using System.Threading.Tasks;
using System.Windows.Input;
using ATC4_HQ.Models; // 引入 GameModel 的命名空间

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
            //输入输出UTF-8
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            StartGameCommand = new RelayCommand(OnStartGame);
            InstallGameCommand = new RelayCommand(OnInstallGame); 
            SettingCommand = new RelayCommand(OnSetting);
        }

        private void OnStartGame()
        {
            Console.WriteLine("ViewModel: 启动游戏逻辑。");
            // 实现启动游戏的业务逻辑
            // 将 CurrentPage 设置为 GameStartOptionsViewModel 的实例
            CurrentPage = new GameStartOptionsViewModel(this); 
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

        // 处理游戏安装和解压的通用方法，现在接收 GameModel 对象
        public async Task HandleInstallGameAndUnzipAsync(GameModel gameData) // ⭐️ 确保方法是 public 且接收 GameModel
        {
            Console.WriteLine($"MainWindowViewModel 接收到游戏数据并准备执行 IPC: 名称={gameData.Name}, 路径={gameData.Path}");
            
            // 1. 发送 ADD_GAME 命令给服务端
            // 参数格式: <Name>|<Path> (不再包含 CoverImagePath)
            string addGameParameter = $"{gameData.Name}|{gameData.Path}"; 
            await SendIpcCommandAsync("ADD_GAME", addGameParameter); 
            Console.WriteLine("已发送 ADD_GAME 命令。");

            // 2. 发送 UNZIP 命令给服务端
            // 参数格式: <Path>
            await SendIpcCommandAsync("UNZIP", gameData.Path); 
            Console.WriteLine("已发送 UNZIP 命令。");
        }

        // 一个通用的公共方法，用于发送带命令和参数的 IPC 消息
        public async Task SendIpcCommandAsync(string command, string parameter)
        {
            // 构造完整的消息字符串，命令和参数之间用空格分隔
            string fullMessage = $"{command} {parameter}";
            await SendRawMessageViaIpcAsync(fullMessage);
        }

        // 实际进行命名管道通信的底层方法
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

    // GameStartOptionsViewModel 的定义，它必须在 ATC4_HQ.ViewModels 命名空间内
    public partial class GameStartOptionsViewModel : ViewModelBase
    {
        // 用于引用 MainWindowViewModel 的字段，以便调用 IPC 方法
        private readonly MainWindowViewModel _mainWindowViewModel;

        public ICommand Button1Command { get; } // 启动上一次游戏
        public ICommand Button2Command { get; } // 列出全部游戏

        // 修改构造函数：接收 MainWindowViewModel 实例
        public GameStartOptionsViewModel(MainWindowViewModel mainWindowViewModel)
        {
            _mainWindowViewModel = mainWindowViewModel; // 保存引用
            Button1Command = new RelayCommand(OnLaunchLastGame);
            Button2Command = new RelayCommand(OnListAllGames);
        }

        private void OnLaunchLastGame()
        {
            Console.WriteLine("Game Start Options: 第一个按钮被点击了！尝试启动上一次游戏。");
            // TODO: 在这里实现启动上一次游戏的逻辑。
            // 这可能需要从服务端获取“上一次游戏”的信息，或者从客户端本地存储中读取。
            // 目前，由于 IPC 是单向的，我们只能在服务端日志中看到结果。
            // 如果需要客户端启动游戏，需要获取游戏路径。
            // 暂时只是打印到控制台。
            // 示例：假设您知道上一次游戏的ID是1，可以这样发送命令（但目前服务端不会返回路径）
            // _mainWindowViewModel.SendIpcCommandAsync("LAUNCH_GAME", "1"); 
        }

        private async void OnListAllGames()
        {
            Console.WriteLine("Game Start Options: 第二个按钮被点击了！尝试列出全部游戏。");
            // 发送 GET_GAMES 命令给服务端
            await _mainWindowViewModel.SendIpcCommandAsync("GET_GAMES", ""); 
            Console.WriteLine("游戏列表请求已发送至服务端。请查看服务端日志 (pipe_server_log.txt) 获取详细列表。");
            // TODO: 如果未来需要将游戏列表返回到客户端显示，需要修改 IPC 为双向通信
        }
    }
}
