using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ATC4_HQ.ViewModels;
using Avalonia.Markup.Xaml;

namespace ATC4_HQ.Views;

public partial class GameListView : UserControl
{
    public GameListView()
    {
        InitializeComponent();
        DataContext = new GameListViewModel();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void GameListBox_DoubleTapped(object? sender, RoutedEventArgs e)
    {
        if (DataContext is GameListViewModel viewModel && viewModel.SelectedGame != null)
        {
            // 用户双击了一个游戏项目，调用GameSelectedCommand
            viewModel.GameSelectedCommand.Execute(null);
        }
    }
}
