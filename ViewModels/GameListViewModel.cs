using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using Masuit.Tools.Files;
using master.Globals;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace ATC4_HQ.ViewModels;

public partial class GameListViewModel : ViewModelBase
{
    private ObservableCollection<string>? _allGames;
    private string? _selectedGame;

    public ObservableCollection<string> AllGames
    {
        get { return _allGames; }
        set { SetProperty(ref _allGames, value); }
    }

    public string? SelectedGame
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
            gameStart.StartGame(SelectedGame);
        }
    }

    public GameListViewModel()
    {
        //从配置文件找要显示的东西
        IniFile ini = new IniFile(GlobalPaths.GamePath + @"\GameData.ini");
        var GameName = ini.GetValue("GameSettings", "GameName");
                
        //显示的内容
        AllGames = new ObservableCollection<string>
        {
            GameName,
        };
    }
}
