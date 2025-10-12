using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using System.Threading.Tasks;

namespace ATC4_HQ.Views
{
    public partial class BtConsentDialog : Window
    {
        private Button? _okButton;
        private Button? _cancelButton;
        private CheckBox? _consentCheckBox;

        public BtConsentDialog()
        {
            InitializeComponent();
            _okButton = this.FindControl<Button>("OkButton");
            _cancelButton = this.FindControl<Button>("CancelButton");
            _consentCheckBox = this.FindControl<CheckBox>("ConsentCheckBox");
            if (_okButton != null) _okButton.Click += OkButton_Click;
            if (_cancelButton != null) _cancelButton.Click += CancelButton_Click;
            if (_consentCheckBox != null)
            {
                _consentCheckBox.IsCheckedChanged += ConsentCheckBox_Changed;
            }
        }

        private void ConsentCheckBox_Changed(object? sender, RoutedEventArgs e)
        {
            if (_okButton != null && _consentCheckBox != null)
                _okButton.IsEnabled = _consentCheckBox.IsChecked == true;
        }

        private void OkButton_Click(object? sender, RoutedEventArgs e)
        {
            Close(true);
        }

        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            Close(false);
        }

        public async Task<bool> ShowDialogAsync(Window parent)
        {
            var result = await this.ShowDialog<bool>(parent);
            return result;
        }
    }
}