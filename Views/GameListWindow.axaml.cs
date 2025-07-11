using Avalonia;
using Avalonia.Controls;
using ATC4_HQ.ViewModels;
using Avalonia.Markup.Xaml;

namespace ATC4_HQ.Views;

public partial class GameListWindow : Window
{
    public GameListWindow()
    {
        InitializeComponent();
        DataContext = new AllGameListViewModel();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}