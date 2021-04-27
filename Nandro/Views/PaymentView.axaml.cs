using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Nandro.ViewModels;

namespace Nandro.Views
{
    public class PaymentView : ReactiveUserControl<PaymentViewModel>
    {
        public TextBox AmountTextBox { get; set; }

        public PaymentView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            var amountTextBox = this.Find<TextBox>("AmountTextBox");
            amountTextBox.AttachedToVisualTree += (_, _) => amountTextBox.Focus();
        }
    }
}
