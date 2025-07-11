using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

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
        AllGames = new ObservableCollection<string>
        {
            "a",
            "b"
        };
    }
}