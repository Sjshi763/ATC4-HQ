using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ATC4_HQ.ViewModels
{
    public partial class AddDownloadDialogViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _magnetLink = string.Empty;

        [ObservableProperty]
        private string _savePath = "Downloads";

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public bool IsValid => !string.IsNullOrWhiteSpace(Name) && 
                               !string.IsNullOrWhiteSpace(MagnetLink) &&
                               MagnetLink.StartsWith("magnet:");

        public AddDownloadDialogViewModel()
        {
        }

        public bool ValidateInput()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Name))
            {
                ErrorMessage = "请输入下载名称";
                return false;
            }

            if (string.IsNullOrWhiteSpace(MagnetLink))
            {
                ErrorMessage = "请输入磁力链接";
                return false;
            }

            if (!MagnetLink.StartsWith("magnet:"))
            {
                ErrorMessage = "请输入有效的磁力链接（以magnet:开头）";
                return false;
            }

            return true;
        }
    }
}
