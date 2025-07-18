using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using master.Globals;
using Masuit.Tools.Files;

namespace ATC4_HQ.ViewModels;

public partial class SettingViewModel : ViewModelBase
{
    [ObservableProperty]
    private MainWindowViewModel _mainWindowViewModel;

    private readonly IniFile _ini;
    private string _LE_address;

    public SettingViewModel()
    {
        _ini = new IniFile(GlobalPaths.InitiatorProfileName);
        _LE_address = GlobalPaths.TransitSoftwareLE;
        if (_LE_address == null)
        {
            _LE_address = "没有LE";
        }
        if (_LE_address == "null")
        {
            _LE_address = "没有LE";
        }
    }

    public string LE_address
    {
        get => _LE_address;
        set
        {
            _LE_address = value;

            OnPropertyChanged();
        }
    }

    public ICommand SaveSettingCommand => new RelayCommand(SaveSetting);

    // 在保存后添加验证
    private void SaveSetting()
    {
        GlobalPaths.TransitSoftwareLE = LE_address;
        _ini.SetValue("main", "TransitSoftwareLE", GlobalPaths.TransitSoftwareLE);
        _ini.Save();
        Console.WriteLine($"设置已保存。{GlobalPaths.TransitSoftwareLE}");
    }
    
    //TODO: 浏览LE地址
}