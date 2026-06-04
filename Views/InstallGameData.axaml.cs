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
                viewModel.InstallGameDataCompleted += OnInstallGameDataCompleted;
                viewModel.ClearSubPageRequested += OnClearSubPageRequested;
            }
        }

        private void InstallGameData_Unloaded(object? sender, RoutedEventArgs e)
        {
            if (DataContext is InstallGameDataViewModel viewModel)
            {
                viewModel.RequestOpenFilePicker -= OnRequestOpenFilePicker;
                viewModel.RequestSaveFileDialog -= OnRequestSaveFileDialog; // 取消订阅保存文件对话框事件
                viewModel.InstallGameDataCompleted -= OnInstallGameDataCompleted;
                viewModel.ClearSubPageRequested -= OnClearSubPageRequested;
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

        private async void OnInstallGameDataCompleted(object? sender, InstallGameDataCompletedEventArgs e)
        {
            if (e.Success && e.GameData != null)
            {
                LoggerHelper.LogInformation($"获取到的游戏数据 - 名称: {e.GameData.Name}, 路径: {e.GameData.Path}");

                if (TopLevel.GetTopLevel(this) is Window mainWindow && mainWindow.DataContext is MainWindowViewModel mainWindowViewModel)
                {
                    await mainWindowViewModel.HandleInstallGameAndUnzipAsync(e.GameData);
                    LoggerHelper.LogInformation("游戏数据已成功传递给 MainWindowViewModel 进行处理。");
                }
                else
                {
                    LoggerHelper.LogError("错误：无法获取 MainWindowViewModel 来处理游戏数据。");
                }
            }
            else
            {
                LoggerHelper.LogInformation("用户取消了安装或安装失败。");
            }
        }

        private void OnClearSubPageRequested(object? sender, EventArgs e)
        {
            if (TopLevel.GetTopLevel(this) is Window mainWindow && mainWindow.DataContext is MainWindowViewModel mainWindowViewModel)
            {
                mainWindowViewModel.ClearSubPage();
                LoggerHelper.LogInformation("已清除右边区域的内容。");
            }
        }

        public async Task<string?> OpenFolderDialog()
        {
            var topLevel = TopLevel.GetTopLevel(this);

            if (topLevel?.StorageProvider is { } storageProvider)
            {
                var folders = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                {
                    Title = "请选择包含 ATC4.z01 到 ATC4.z08 和 ATC4.zip 的完整文件夹",
                    AllowMultiple = false
                });

                if (folders.Count >= 1)
                {
                    var selectedFolderUri = folders[0].Path;
                    try
                    {
                        var selectedPath = selectedFolderUri.LocalPath;
                        LoggerHelper.LogInformation($"已选择文件夹: {selectedPath}"); // Console 错误会解决
                        return selectedPath;
                    }
                    catch (InvalidOperationException)
                    {
                        LoggerHelper.LogError("选定的位置不是本地文件路径。"); // Console 错误会解决
                        return null;
                    }
                }
                else
                {
                    LoggerHelper.LogInformation("未选择任何文件夹"); // Console 错误会解决
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
                        LoggerHelper.LogInformation($"已选择保存路径: {selectedPath}");
                        return selectedPath;
                    }
                    catch (InvalidOperationException)
                    {
                        LoggerHelper.LogError("选定的位置不是本地文件路径。");
                        return null;
                    }
                }
                else
                {
                    LoggerHelper.LogInformation("未选择保存位置");
                    return null;
                }
            }
            return null;
        }

    }
}
