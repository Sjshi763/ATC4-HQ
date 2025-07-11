using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
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

    private void GameListBox_DoubleTapped(object sender, RoutedEventArgs e)
    {
        if (DataContext is AllGameListViewModel viewModel && viewModel.SelectedGame != null)
        {
            // 用户双击了一个游戏项目，返回选择的游戏并关闭窗口
            Close(viewModel.SelectedGame);
        }
    }
}