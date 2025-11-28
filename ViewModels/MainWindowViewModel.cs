using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using ATC4_HQ.Models; // 引入 GameModel 的命名空间
using System.IO.Compression; // 用于解压缩功能
using master.Globals;
using Masuit.Tools.Files;
using System.IO; // 用于检查文件是否存在

namespace ATC4_HQ.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ViewModelBase? _currentPage; // 当前显示在 PageHost 中的 ViewModel
        
        // 事件：当需要显示OPENAL未安装警告时触发
        public event EventHandler? OpenALNotInstalled;
        
        // 事件：当需要显示OPENAL安装界面时触发
        public event EventHandler? ShowOpenALInstallView;
        
        public ICommand StartGameCommand { get; }
        public ICommand InstallGameCommand { get; } // 用于 ViewModel 内部逻辑或未来绑定
        public ICommand SettingCommand { get; }

        public MainWindowViewModel()
        {
            StartGameCommand = new RelayCommand(OnStartGame);
            InstallGameCommand = new RelayCommand(OnInstallGame); 
            SettingCommand = new RelayCommand(OnSetting);
        }

        private void OnStartGame()
        {
            Console.WriteLine("ViewModel: 启动游戏逻辑。");
            
            // 检查OPENAL是否安装
            if (!IsOpenALInstalled())
            {
                Console.WriteLine("ViewModel: OPENAL未安装，触发显示安装界面事件。");
                // 触发事件通知View显示OpenAL安装界面
                ShowOpenALInstallView?.Invoke(this, EventArgs.Empty);
                return; // 不继续执行后续逻辑
            }
            
            // 实现启动游戏的业务逻辑
            // 将 CurrentPage 设置为 GameStartOptionsViewModel 的实例
            CurrentPage = new GameStartOptionsViewModel(this); 
            Console.WriteLine("ViewModel: 已切换到游戏启动选项界面。");
        }

        private void OnInstallGame()
        {
            Console.WriteLine("ViewModel: 安装游戏按钮内部逻辑（如果需要）。");
            // 备注：打开对话框的逻辑在 MainWindow.axaml.cs 的 InstallGame_Click 中
            // 这个方法可以用于按钮的内部逻辑，或者当您把 InstallGame_Click 改为 Command 绑定时使用
        }

        private void OnSetting()
        {
            Console.WriteLine("启动设置");
            CurrentPage = new SettingViewModel(); 
        }

        /// <summary>
        /// 检查OPENAL是否安装
        /// </summary>
        /// <returns>如果OPENAL已安装返回true，否则返回false</returns>
        private bool IsOpenALInstalled()
        {
            // 检查系统目录中是否存在openal32.dll
            string openalPath = Path.Combine(Environment.SystemDirectory, "openal32.dll");
            return File.Exists(openalPath);
        }

        // 处理游戏安装和解压的通用方法，现在接收 GameModel 对象
        public async Task HandleInstallGameAndUnzipAsync(GameModel gameData) // ⭐️ 确保方法是 public 且接收 GameModel
        {
            string zipPath = @"B:\XIANGMU\ATC4-HQ\ATC4ALL.zip";
            // 解压 .zip 文件到指定目录
            ZipFile.ExtractToDirectory(zipPath, gameData.Path);

            // 检查解压后的文件中是否有以~开头的zip文件，并再次解压
            var extractedFiles = Directory.GetFiles(gameData.Path, "*.zip", SearchOption.AllDirectories);
            foreach (var file in extractedFiles)
            {
                var fileName = Path.GetFileName(file);
                if (fileName.StartsWith("~"))
                {
                    // 使用一个新的 MemoryStream 来读取和解压，避免文件占用问题
                    using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await fileStream.CopyToAsync(memoryStream);
                            memoryStream.Position = 0; // 重置流的位置

                            using (ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Read))
                            {
                                foreach (ZipArchiveEntry entry in archive.Entries)
                                {
                                    // 去掉条目前面的波浪号，构造正确的目标路径
                                    string destinationPath = Path.Combine(gameData.Path, entry.FullName.TrimStart('~'));
                                    
                                    // 确保目标目录存在
                                    string? destinationDirectory = Path.GetDirectoryName(destinationPath);
                                    if (destinationDirectory != null)
                                    {
                                        Directory.CreateDirectory(destinationDirectory);
                                    }

                                    // 如果不是目录，则解压文件
                                    if (!string.IsNullOrEmpty(entry.Name))
                                    {
                                        entry.ExtractToFile(destinationPath, true);
                                    }
                                }
                            }
                        }
                    }
                    File.Delete(file); // 解压完成后删除这个临时的zip文件
                }
            }

            GlobalPaths.GamePath = gameData.Path; // 更新全局路径
            IniFile ini = new IniFile(GlobalPaths.InitiatorProfileName);
            ini.SetValue("main", "GamePath", GlobalPaths.GamePath);
            ini.Save();
            GlobalPaths.GameName = gameData.Name;
            ini = new IniFile(GlobalPaths.GamePath + @"\GameData.ini");
            ini.SetValue("GameSettings" , "GameName" , GlobalPaths.GameName);
            ini.Save();
        }
    }
}
