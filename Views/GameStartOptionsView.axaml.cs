using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using System;
using ATC4_HQ.Views;
using ATC4_HQ.ViewModels;
using Microsoft.Extensions.Logging;

namespace ATC4_HQ.Views
{
    public partial class GameStartOptionsView : UserControl
    {
        private MainWindowViewModel? _mainWindowViewModel;

        public GameStartOptionsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void Button2(object? sender, RoutedEventArgs e)
        {
            LoggerHelper.LogInformation("列出全部游戏按钮被点击了，在右边区域显示游戏列表。");
            
            // 获取MainWindowViewModel的引用
            if (this.DataContext is GameStartOptionsViewModel gameStartOptionsViewModel)
            {
                // 通过反射获取MainWindowViewModel的私有字段
                var fieldInfo = typeof(GameStartOptionsViewModel).GetField("_mainWindowViewModel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (fieldInfo != null)
                {
                    _mainWindowViewModel = fieldInfo.GetValue(gameStartOptionsViewModel) as MainWindowViewModel;
                }
            }
            
            // 在右边区域显示游戏列表
            if (_mainWindowViewModel != null)
            {
                _mainWindowViewModel.CurrentSubPage = new GameListViewModel();
            }
        }
    }
}
