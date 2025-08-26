using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ATC4_HQ.ViewModels;

namespace ATC4_HQ.Views
{
    public partial class OpenALInstallView : UserControl
    {
        public OpenALInstallView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
