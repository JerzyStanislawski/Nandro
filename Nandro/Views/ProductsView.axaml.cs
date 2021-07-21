using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Nandro.ViewModels;

namespace Nandro.Views
{
    public partial class ProductsView : ReactiveUserControl<ProductsViewModel>
    {
        public ProductsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            this.FindControl<DataGrid>("DataGrid").RowEditEnded += ProductsView_RowEditEnded;
        }

        private void ProductsView_RowEditEnded(object sender, DataGridRowEditEndedEventArgs e)
        {            
            ViewModel.RaisePropertyChanged();
        }
    }
}
