using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.IO.Pipes; // 用于命名管道
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ATC4_HQ.ViewModels
{
    public partial class InstallGameViewModel : ViewModelBase
    {
        // 用于请求 View 打开 InstallGameDataDialogWindow 的事件
        public event EventHandler RequestOpenInstallGameDataDialog;

        public ICommand InstallGameCommand { get; }

        public InstallGameViewModel()
        {
            InstallGameCommand = new RelayCommand(OnInstallGame);
        }

        private void OnInstallGame()
        {
            // 触发事件，请求 View 打开对话框
            RequestOpenInstallGameDataDialog?.Invoke(this, EventArgs.Empty);
        }

        // --- IPC 逻辑 ---
        // 这个方法将由 InstallGame.axaml.cs 在获取到对话框结果后调用
        public async Task SendPathViaIpcAsync(string path)
        {
            Console.WriteLine($"尝试通过 IPC 发送路径: {path}");
            try
            {
                using var pipeClient = new NamedPipeClientStream(".", "ATC4Pipe", PipeDirection.Out, PipeOptions.Asynchronous);
                // 客户端连接超时时间，例如 5 秒
                await pipeClient.ConnectAsync(5000); 
                
                if (pipeClient.IsConnected)
                {
                    byte[] messageBytes = Encoding.UTF8.GetBytes(path);
                    await pipeClient.WriteAsync(messageBytes, 0, messageBytes.Length);
                    Console.WriteLine("路径已通过 IPC 发送。");
                    // 如果需要接收响应，这里可以添加 ReadAsync 逻辑
                    // byte[] buffer = new byte[256];
                    // int bytesRead = await pipeClient.ReadAsync(buffer, 0, buffer.Length);
                    // string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    // Console.WriteLine($"IPC 响应: {response}");
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