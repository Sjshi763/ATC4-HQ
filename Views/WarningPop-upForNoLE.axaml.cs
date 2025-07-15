using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using System.Threading.Tasks;

namespace ATC4_HQ.Views;

public partial class WarningPop_upForNoLE : Window
{
    private bool _dialogResult;
    
    public WarningPop_upForNoLE()
    {
        InitializeComponent();
        
        _dialogResult = false;
        this.Closed += (s, e) => 
        {
            _dialogResult = true;
        };
        
#if DEBUG
        this.AttachDevTools();
#endif
    }
    
    private void Close_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
    
    public async Task<bool> ShowDialog(Window parent)
    {
        await base.ShowDialog(parent); // 使用 base 调用父类的 ShowDialog
        return _dialogResult;
    }
}