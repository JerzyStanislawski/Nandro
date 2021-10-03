using Nandro.Data;
using Nandro.Models;
using Nandro.Nano;
using Nandro.Providers;
using ReactiveUI;
using ReactiveUI.Validation.Helpers;
using Splat;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace Nandro.ViewModels
{
    public class CartViewModel : ReactiveValidationObject, IRoutableViewModel
    {
        private readonly NandroDbContext _dbContext;
        private readonly PriceProvider _priceProvider;
        private readonly CurrencyProvider _currencyProvider;

        public IScreen HostScreen { get; }
        public string UrlPathSegment { get; } = Guid.NewGuid().ToString().Substring(0, 5);

        public ReactiveCommand<Unit, IRoutableViewModel> Proceed { get; }

        public ReactiveCommand<Unit, Unit> Add => ReactiveCommand.Create(AddCartItem, canExecute: Observable.Return(CanAdd));

        public ReactiveCommand<CartItem, Unit> Remove => ReactiveCommand.Create<CartItem>(RemoveCartItem);

        public ReactiveCommand<CartItem, Unit> Increment => ReactiveCommand.Create<CartItem>(IncrementCount);

        public ReactiveCommand<CartItem, Unit> Decrement => ReactiveCommand.Create<CartItem>(DecrementCount);

        public ObservableCollection<CartItem> CartItems { get; }
        public ObservableCollection<Product> Products { get; }
        public string PriceString => $"Price [{_dbContext.Configuration.Single().CurrencyCode}]";
        public string TotalPriceText => $"Total Price [{_dbContext.Configuration.Single().CurrencyCode}]:";
        public string TotalPriceValueText => $"{TotalPrice.ToString("0.00")} {_currencyProvider.DefaultCurrency.Symbol}";
        public string TotalNanoPriceValueText => $"{TotalNanoPrice.ToString("0.00")} NANO";
        public bool CanProceed { get; private set; }
        public bool CanAdd { get; private set; } = true;
        public decimal TotalPrice { get; private set; }
        public decimal TotalNanoPrice { get; private set; }

        public EventHandler<CartItemRowAddedEventArgs> RowAdded;

        public CartViewModel(IScreen screen)
        {
            HostScreen = screen;

            Proceed = ReactiveCommand.CreateFromObservable(RequestPayment);

            _dbContext = Locator.Current.GetService<NandroDbContext>();
            _priceProvider = Locator.Current.GetService<PriceProvider>();
            _currencyProvider = Locator.Current.GetService<CurrencyProvider>();

            CartItems = new ObservableCollection<CartItem>();
            Products = new ObservableCollection<Product>(_dbContext.Products);
        }

        private IObservable<IRoutableViewModel> RequestPayment()
        {
            var nanoAccount = _dbContext.Configuration.Single().NanoAccount;
            return HostScreen.Router.Navigate.Execute(new TransactionViewModel(HostScreen, nanoAccount, Tools.ToRaw(TotalNanoPrice)));
        }

        public void ProductChanged(int index, Product product)
        {
            var cartItem = CartItems[index];

            cartItem.Price = product.Price;
            cartItem.Unit = product.Unit;
            cartItem.Product = product;

            cartItem.RaisePropertyChanged(nameof(cartItem.Price));
            cartItem.RaisePropertyChanged(nameof(cartItem.Unit));

            RefreshTotalPrice();
            RefreshAddButton();
        }

        private void AddCartItem()
        {
            var item = new CartItem
            {
                Count = 1m
            };

            item.CountChanged += Count_PropertyChanged;

            CartItems.Add(item);

            RowAdded?.Invoke(this, new CartItemRowAddedEventArgs
            {
                CartItem = item
            });
            RefreshAddButton();
        }

        private void Count_PropertyChanged(object sender, EventArgs e)
        {
            RefreshTotalPrice();
        }

        private void RemoveCartItem(CartItem cartItemToRemove)
        {
            CartItems.Remove(cartItemToRemove);
            RefreshTotalPrice();
            RefreshAddButton();
        }

        private void RefreshTotalPrice()
        {
            TotalPrice = CartItems.Sum(x => x.Count * x.Price);
            TotalNanoPrice = _priceProvider.CurrencyToNano(TotalPrice);

            this.RaisePropertyChanged(nameof(TotalPrice));
            this.RaisePropertyChanged(nameof(TotalNanoPrice));
            this.RaisePropertyChanged(nameof(TotalPriceValueText));
            this.RaisePropertyChanged(nameof(TotalNanoPriceValueText));

            CanProceed = TotalNanoPrice > 0;
            this.RaisePropertyChanged(nameof(CanProceed));
        }

        private void IncrementCount(CartItem ci)
        {
            ci.Count++;
            ci.RaisePropertyChanged(nameof(ci.Count));
        }

        private void DecrementCount(CartItem ci)
        {
            ci.Count = ci.Count < 1 ? 0 : ci.Count - 1;
            ci.RaisePropertyChanged(nameof(ci.Count));
        }

        private void RefreshAddButton()
        {
            CanAdd = CartItems.All(x => x.Product != null);
            this.RaisePropertyChanged(nameof(Add));
        }
    }

    public class CartItemRowAddedEventArgs : EventArgs
    {
        public CartItem CartItem { get; set; }
    }
}
