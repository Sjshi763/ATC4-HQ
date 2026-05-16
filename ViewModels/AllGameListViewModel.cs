using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using SoftCircuits.IniFileParser;
using master.Globals;
using System.IO;

namespace ATC4_HQ.ViewModels;

public class AllGameListViewModel : ObservableObject
{
    private ObservableCollection<string>? _AllGames;
    private string? _selectedGame;

    public ObservableCollection<string> AllGames
    {
        get { return _AllGames; }
        set { SetProperty(ref _AllGames, value); }
    }

    public string? SelectedGame
    {
        get { return _selectedGame; }
        set { SetProperty(ref _selectedGame, value); }
    }

    public AllGameListViewModel()
    {
        //从配置文件找要显示的东西
        var gameDataIniPath = GlobalPaths.GamePath + @"\GameData.ini";
        IniFile ini = new IniFile();
        if (File.Exists(gameDataIniPath))
        {
            ini.Load(gameDataIniPath);
        }
        var GameName = ini.GetSetting("GameSettings", "GameName", string.Empty) ?? string.Empty;
                
        //显示的内容
        AllGames = new ObservableCollection<string>
        {
            GameName,
        };
    }
}