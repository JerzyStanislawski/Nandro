using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Nandro.ViewModels;

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

            _dataGrid = this.FindControl<DataGrid>("DataGrid");
            _dataGrid.RowEditEnded += ProductsView_RowEditEnded;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);       
        }

        private void ProductsView_RowEditEnded(object sender, DataGridRowEditEndedEventArgs e)
        {            
            ViewModel.RaisePropertyChanged();
        }
    }
}
