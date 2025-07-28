using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ATC4_HQ.ViewModels
{
    public partial class InstallGameViewModel : ViewModelBase
    {
        // 用于请求 View 打开 InstallGameDataDialogWindow 的事件
        public event EventHandler RequestOpenInstallGameDataDialog;

        public ICommand InstallGameCommand { get; }

        public InstallGameViewModel()
        {
            InstallGameCommand = new RelayCommand(OnInstallGame);
        }

        private void OnInstallGame()
        {
            // 触发事件，请求 View 打开对话框
            RequestOpenInstallGameDataDialog?.Invoke(this, EventArgs.Empty);
        }


    }
}