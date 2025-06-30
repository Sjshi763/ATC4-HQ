using System;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Interactivity;
using System.Threading.Tasks;
namespace ATC4_HQ.Views
{
    public partial class InstallGameData : UserControl
    {
        public InstallGameData()
        {
            InitializeComponent();
        }

        private async void FindFile_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            string? driveLetter = await OpenFolderDialog(); //返回用户选择的文件夹路径
            var browseButton = sender as Button;
            //判断一下用户到底有没有选择文件夹
            if (driveLetter != null)
            {
                // 如果用户选择了文件夹，就将这个路径赋值给输入框
                this.WhereIsTheGames.Text = driveLetter;
            }
            else
            {
                this.WhereIsTheGames.Text = "未选择任何文件夹";
            }
        }

        public async System.Threading.Tasks.Task<string?> OpenFolderDialog()
        {
            // 获取当前窗口的顶级对象
            var topLevel = TopLevel.GetTopLevel(this);

            // 检查 StorageProvider 是否可用
            if (topLevel?.StorageProvider is { } storageProvider)
            {
                // 打开文件夹选择对话框
                var folders = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                {
                    Title = "请选择游戏使用路径",
                    AllowMultiple = false // 设置为 true 可以选择多个文件夹
                });

                // 检查是否选择了文件夹
                if (folders.Count >= 1)
                {
                    // 获取所选文件夹的本地路径
                    var selectedFolderUri = folders[0].Path;

                    try
                    {
                        // 尝试获取 LocalPath，如果失败则会抛出异常
                        var selectedPath = selectedFolderUri.LocalPath;
                        Console.WriteLine($"已选择文件夹: {selectedPath}");
                        return selectedPath;
                    }
                    catch (InvalidOperationException)
                    {
                        // 如果是相对 URI 或其他不支持的操作，捕获异常并返回 null
                        Console.WriteLine("选定的位置不是本地文件路径。");
                        return null;
                    }
                }
                else
                {
                    Console.WriteLine("未选择任何文件夹");
                    return null;
                }
            }
            return null; // 如果 StorageProvider 不可用，返回 null
        }

        private void SaveButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            
        }
    }
}