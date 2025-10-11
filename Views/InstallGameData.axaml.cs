using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Interactivity;
using System.Threading.Tasks;
using ATC4_HQ.ViewModels;
using System; // 添加此 using 引用，用于 EventArgs 和 Console

namespace ATC4_HQ.Views
{
    public partial class InstallGameData : UserControl
    {
        public InstallGameData()
        {
            InitializeComponent();
            this.Loaded += InstallGameData_Loaded;
            this.Unloaded += InstallGameData_Unloaded; // 添加 Unloaded 事件处理，以取消订阅
        }

        private void InstallGameData_Loaded(object? sender, RoutedEventArgs e)
        {
            if (DataContext is InstallGameDataViewModel viewModel)
            {
                viewModel.RequestOpenFilePicker += OnRequestOpenFilePicker;
                viewModel.RequestSaveFileDialog += OnRequestSaveFileDialog; // 添加保存文件对话框事件处理
            }
        }

        private void InstallGameData_Unloaded(object? sender, RoutedEventArgs e)
        {
            if (DataContext is InstallGameDataViewModel viewModel)
            {
                viewModel.RequestOpenFilePicker -= OnRequestOpenFilePicker;
                viewModel.RequestSaveFileDialog -= OnRequestSaveFileDialog; // 取消订阅保存文件对话框事件
            }
        }

        private async void OnRequestOpenFilePicker(object? sender, EventArgs e)
        {
            if (DataContext is InstallGameDataViewModel viewModel)
            {
                string? driveLetter = await OpenFolderDialog();
                viewModel.GamePath = driveLetter ?? "未选择任何文件夹";
            }
        }

        private async void OnRequestSaveFileDialog(object? sender, SaveFileDialogEventArgs e)
        {
            if (DataContext is InstallGameDataViewModel viewModel)
            {
                string? selectedPath = await OpenSaveFileDialog();
                e.SetResult(selectedPath);
            }
        }

        public async Task<string?> OpenFolderDialog()
        {
            var topLevel = TopLevel.GetTopLevel(this);

            if (topLevel?.StorageProvider is { } storageProvider)
            {
                var folders = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                {
                    Title = "请选择游戏使用路径",
                    AllowMultiple = false
                });

                if (folders.Count >= 1)
                {
                    var selectedFolderUri = folders[0].Path;
                    try
                    {
                        var selectedPath = selectedFolderUri.LocalPath;
                        Console.WriteLine($"已选择文件夹: {selectedPath}"); // Console 错误会解决
                        return selectedPath;
                    }
                    catch (InvalidOperationException)
                    {
                        Console.WriteLine("选定的位置不是本地文件路径。"); // Console 错误会解决
                        return null;
                    }
                }
                else
                {
                    Console.WriteLine("未选择任何文件夹"); // Console 错误会解决
                    return null;
                }
            }
            return null;
        }

        public async Task<string?> OpenSaveFileDialog()
        {
            var topLevel = TopLevel.GetTopLevel(this);

            if (topLevel?.StorageProvider is { } storageProvider)
            {
                var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = "请选择游戏文件保存位置",
                    DefaultExtension = ".zip",
                    FileTypeChoices = new FilePickerFileType[]
                    {
                        new FilePickerFileType("ZIP文件")
                        {
                            Patterns = new[] { "*.zip" }
                        },
                        new FilePickerFileType("所有文件")
                        {
                            Patterns = new[] { "*.*" }
                        }
                    }
                });

                if (file != null)
                {
                    try
                    {
                        var selectedPath = file.Path.LocalPath;
                        Console.WriteLine($"已选择保存路径: {selectedPath}");
                        return selectedPath;
                    }
                    catch (InvalidOperationException)
                    {
                        Console.WriteLine("选定的位置不是本地文件路径。");
                        return null;
                    }
                }
                else
                {
                    Console.WriteLine("未选择保存位置");
                    return null;
                }
            }
            return null;
        }
    }
}
