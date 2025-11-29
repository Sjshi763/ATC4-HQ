using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ATC4_HQ.Views
{
    public partial class DownloadMonitorView : UserControl
    {
        public DownloadMonitorView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
