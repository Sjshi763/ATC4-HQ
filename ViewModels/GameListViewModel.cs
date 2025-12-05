using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using Masuit.Tools.Files;
using master.Globals;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using System.IO;

namespace ATC4_HQ.ViewModels;

public class GameInfo
{
    public string Name { get; set; }
    public string Path { get; set; }
    
    public override string ToString()
    {
        return Name;
    }
}

public partial class GameListViewModel : ViewModelBase
{
    private ObservableCollection<GameInfo>? _allGames;
    private GameInfo? _selectedGame;

    public ObservableCollection<GameInfo> AllGames
    {
        get { return _allGames; }
        set { SetProperty(ref _allGames, value); }
    }

    public GameInfo? SelectedGame
    {
        get { return _selectedGame; }
        set { SetProperty(ref _selectedGame, value); }
    }

    [RelayCommand]
    private void GameSelected()
    {
        if (SelectedGame != null)
        {
            // 用户选择了一个游戏项目，调用GameStart处理
            var gameStart = new GameStart();
            gameStart.StartGame(SelectedGame.Path);
        }
    }

    public GameListViewModel()
    {
        //从配置文件找要显示的东西
        IniFile ini = new IniFile(GlobalPaths.InitiatorProfileName);
        var games = new ObservableCollection<GameInfo>();
        
        //读取游戏目录配置
        var gameDirs = ini.GetSection("GameDirectories");
        if (gameDirs != null)
        {
            foreach (var key in gameDirs.Keys)
            {
                var path = gameDirs[key];
                if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
                {
                    var gameName = Path.GetFileName(path);
                    games.Add(new GameInfo { Name = gameName, Path = path });
                }
            }
        }
        
        //如果没有找到游戏目录，添加默认项
        if (games.Count == 0)
        {
            games.Add(new GameInfo { Name = "默认游戏", Path = GlobalPaths.GamePath ?? "" });
        }
                
        //显示的内容
        AllGames = games;
    }
}
