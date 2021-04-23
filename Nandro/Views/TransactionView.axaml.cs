using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Nandro.ViewModels;

namespace Nandro.Views
{
    public class TransactionView : ReactiveUserControl<TransactionViewModel>
    {
        public TransactionView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
