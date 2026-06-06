using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ATC4_HQ.ViewModels;

namespace ATC4_HQ.Views
{
    public partial class ExtractProgressWindow : Window
    {
        public ExtractProgressWindow()
        {
            InitializeComponent();
        }

        public ExtractProgressWindow(ExtractProgressViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}