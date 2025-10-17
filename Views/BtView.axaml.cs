using Avalonia.Controls;

namespace ATC4_HQ.Views
{
    public partial class BtView : UserControl
    {
        public BtView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Avalonia.Markup.Xaml.AvaloniaXamlLoader.Load(this);
        }
    }
}
