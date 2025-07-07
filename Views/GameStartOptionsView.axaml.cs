using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ATC4_HQ.Views
{
    public partial class GameStartOptionsView : UserControl
    {
        public GameStartOptionsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}