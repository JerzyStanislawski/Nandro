using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Nandro.ViewModels;

namespace Nandro.Views
{
    public class TransactionResultView : ReactiveUserControl<TransactionResultViewModel>
    {
        public TransactionResultView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
