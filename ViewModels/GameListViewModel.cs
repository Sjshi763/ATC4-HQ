using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using SoftCircuits.IniFileParser;
using master.Globals;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using System.IO;
using System.Threading.Tasks;

namespace ATC4_HQ.ViewModels;

public class GameInfo
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    
    public override string ToString()
    {
        return Name;
    }
}

public partial class GameListViewModel : ViewModelBase
{
    private ObservableCollection<GameInfo> _allGames = new();
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
    private async Task GameSelected()
    {
        if (SelectedGame != null)
        {
            // 用户选择了一个游戏项目，调用GameStart处理
            var gameStart = new GameStart();
            await gameStart.StartGame(SelectedGame.Path);
        }
    }

    public GameListViewModel()
    {
        var games = new ObservableCollection<GameInfo>();
        
        //从全局游戏列表加载
        foreach (var game in GlobalPaths.Games)
        {
            if (!string.IsNullOrEmpty(game.Path) && Directory.Exists(game.Path))
            {
                games.Add(new GameInfo { Name = game.Name, Path = game.Path });
            }
        }
        
        //如果没有找到游戏，添加提示项
        if (games.Count == 0)
        {
            games.Add(new GameInfo { Name = "暂无游戏，请先安装", Path = "" });
        }
                
        //显示的内容
        AllGames = games;
    }
}
