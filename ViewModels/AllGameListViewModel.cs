using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using Masuit.Tools.Files;
using master.Globals;

namespace ATC4_HQ.ViewModels;

public class AllGameListViewModel : ObservableObject
{
    private ObservableCollection<string>? _AllGames;

    public ObservableCollection<string> AllGames
    {
        get { return _AllGames; }
        set { SetProperty(ref _AllGames, value); }
    }

    public AllGameListViewModel()
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