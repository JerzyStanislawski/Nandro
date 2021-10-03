using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Avalonia.VisualTree;
using Nandro.Models;
using Nandro.ViewModels;
using System;
using System.Linq;
using System.Reflection;

namespace Nandro.Views
{
    public class CartView : ReactiveUserControl<CartViewModel>
    {
        private DataGrid _dataGrid;

        public CartView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            _dataGrid = this.FindControl<DataGrid>("DataGrid");
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            _dataGrid.Columns[2].Header = ViewModel.PriceString;
           
            ViewModel.RowAdded += GridRowAdded;
        }

        public void GridRowAdded(object sender, CartItemRowAddedEventArgs args)
        {
            var descendants = _dataGrid.GetVisualDescendants();
            var box = (AutoCompleteBox)descendants
                .SingleOrDefault(x => x is AutoCompleteBox nameControl && nameControl.DataContext == args.CartItem);
            box?.Focus();
        }

        public void ProductSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            //This method is ugly, but I haven't found the other way. AutoCompleteBox.SelectedItem binding didn't work.
            if (args.AddedItems.Count == 0)
                return;

            var productTextBox = (AutoCompleteBox)sender;
            var cell = (DataGridCell)productTextBox.Parent;
            var cellsPresenter = (DataGridCellsPresenter)cell.Parent;

            if (!cell.IsInitialized || cell.Parent is null)
                return;

            var row = (DataGridRow)cellsPresenter.GetType().GetProperty("OwningRow", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(cellsPresenter);
            ViewModel.ProductChanged(row.GetIndex(), (Product)args.AddedItems[0]);

            _dataGrid.Focus();
        }
    }
}
