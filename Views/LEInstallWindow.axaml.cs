using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ATC4_HQ.ViewModels;
using System;
using System.Threading.Tasks;

namespace ATC4_HQ.Views;

public partial class LEInstallWindow : Window
{
    private LEInstallViewModel _viewModel;
    private bool _dialogResult;
    
    public LEInstallWindow()
    {
        InitializeComponent();
        _viewModel = new LEInstallViewModel();
        DataContext = _viewModel;
        
        _dialogResult = false;
        this.Closed += (s, e) => 
        {
            _dialogResult = true;
        };
        
        // 订阅ViewModel的事件
        _viewModel.RequestClose += (s, e) => Close();
        _viewModel.ShowMessage += (s, message) => 
        {
            // 这里可以显示消息框或其他通知方式
            var mainWindow = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop 
                ? desktop.MainWindow 
                : null;
            // 简单的控制台输出，实际项目中可能需要更好的消息显示方式
            Console.WriteLine($"消息: {message}");
        };
        
#if DEBUG
        this.AttachDevTools();
#endif
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    public new async Task<bool> ShowDialog(Window parent)
    {
        await base.ShowDialog(parent);
        return _dialogResult;
    }
}
