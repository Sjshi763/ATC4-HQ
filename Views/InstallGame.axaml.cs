using Avalonia.Controls;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;
namespace ATC4_HQ.Views
{
    public partial class InstallGame : UserControl
    {
        public InstallGame()
        {
            InitializeComponent();
        }

        private async void InstallGame_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
           PageHost.Content = new InstallGameData();
        }

        private async Task SendPipeMessageAsync(string msg)
        {
            using var pipeClient = new NamedPipeClientStream(".", "ATC4Pipe", PipeDirection.InOut, PipeOptions.Asynchronous);
            await pipeClient.ConnectAsync();
            await pipeClient.WriteAsync(Encoding.UTF8.GetBytes(msg));
            byte[] buffer = new byte[256];
            int bytesRead = await pipeClient.ReadAsync(buffer, 0, buffer.Length);
            string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            // 处理响应
        }
    }
}