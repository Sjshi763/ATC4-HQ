using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;

namespace ATC4_HQ.Views;

public partial class WarningPop_up : Window
{
    public WarningPop_up()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    private void Close_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}


    
