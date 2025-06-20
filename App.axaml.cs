using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Controls;
using ATC4_HQ.Views;
using System.Linq;

namespace ATC4_HQ
{
    public partial class App : Application
    {
        // 定义屏幕分辨率大小
        public static int ScreenWidth ; 
        public static int ScreenHeight ;
        // 定义窗口大小变量
        public static double WindowWidth { get; set; } = ScreenWidth / 4;
        public static double WindowHeight { get; set; } = ScreenHeight / 4;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var mainWindow = new MainWindow();
                // 通过 mainWindow.Screens 获取屏幕信息
                var screens = mainWindow.Screens;
                var primaryScreen = screens?.All.OrderByDescending(s => s.IsPrimary).FirstOrDefault();
                if (primaryScreen != null)
                {
                    WindowWidth = primaryScreen.WorkingArea.Width / 4.0;
                    WindowHeight = primaryScreen.WorkingArea.Height / 4.0;
                }
                else
                {
                    WindowWidth = 900;
                    WindowHeight = 600;
                }

                mainWindow.Width = WindowWidth;
                mainWindow.Height = WindowHeight;
                desktop.MainWindow = mainWindow;
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}