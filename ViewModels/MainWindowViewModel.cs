using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using ATC4_HQ.Models; // 引入 GameModel 的命名空间
using System.IO.Compression; // 用于解压缩功能
using master.Globals;
using Masuit.Tools.Files;

namespace ATC4_HQ.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ViewModelBase? _currentPage; // 当前显示在 PageHost 中的 ViewModel
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

        // 处理游戏安装和解压的通用方法，现在接收 GameModel 对象
        public async Task HandleInstallGameAndUnzipAsync(GameModel gameData) // ⭐️ 确保方法是 public 且接收 GameModel
        {
            string zipPath = @"B:\XIANGMU\ATC4-HQ\ATC4ALL.zip";
            // 解压 .zip 文件到指定目录
            ZipFile.ExtractToDirectory(zipPath, gameData.Path);
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