using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using System;
using ATC4_HQ.Views;

namespace ATC4_HQ.Views
{
    public partial class GameStartOptionsView : UserControl
    {
        public GameStartOptionsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private async void Button2(object? sender, RoutedEventArgs e)
        {
            Console.WriteLine("安装游戏按钮被点击了，准备打开对话框。");
            //启动窗口
            var dialogWindow = new GameListWindow();
            // 修复 CS8604 警告：确保 owner 是可空类型或显式转换
            Window? ownerWindow = TopLevel.GetTopLevel(this) as Window;
            bool? dialogResult = await dialogWindow.ShowDialog<bool?>(ownerWindow);
        }
    }
}