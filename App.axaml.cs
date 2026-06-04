using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ATC4_HQ.ViewModels; // 引用您的 ViewModel 命名空间
using ATC4_HQ.Views;     // 引用您的 View 命名空间

namespace ATC4_HQ
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    // 设置 MainWindow 的 DataContext
                    DataContext = new MainWindowViewModel() 
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}