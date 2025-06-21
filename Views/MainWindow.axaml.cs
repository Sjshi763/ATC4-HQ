using Avalonia.Controls;

namespace ATC4_HQ.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // PageHost.Content = new MainWindow(); // 默认显示主页
        }

        private void StartGame_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            PageHost.Content = new GameWindow(); // 切换到新页面
        }

        private void Setting_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            PageHost.Content = new SettingWindow(); // 切换到设置页面
        }
    }
}