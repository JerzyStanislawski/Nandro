using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Avalonia.VisualTree;
using Nandro.ViewModels;
using System.Linq;

namespace Nandro.Views
{
    public partial class ProductsView : ReactiveUserControl<ProductsViewModel>
    {
        private DataGrid _dataGrid;

        public ProductsView()
        {
            InitializeComponent();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            
            _dataGrid.Columns[1].Header = ViewModel.PriceString;

            ViewModel.ProductAdded += GridRowAdded;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            _dataGrid = this.FindControl<DataGrid>("DataGrid");
            _dataGrid.RowEditEnded += ProductsView_RowEditEnded;
        }

        private void ProductsView_RowEditEnded(object sender, DataGridRowEditEndedEventArgs e)
        {            
            ViewModel.RaisePropertyChanged();
        }

        public void GridRowAdded(object sender, ProductRowAddedEventArgs args)
        {
            var descendants = _dataGrid.GetVisualDescendants();
            var box = (TextBox)descendants
                .FirstOrDefault(x => x is TextBox nameControl && nameControl.DataContext == args.Product);
            box?.Focus();
        }
    }
}
