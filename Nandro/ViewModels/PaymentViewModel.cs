using Nandro.Nano;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;
using Splat;
using System;
using System.Reactive;
using System.Reactive.Linq;

namespace Nandro.ViewModels
{
    public class PaymentViewModel : ReactiveValidationObject, IRoutableViewModel
    {
        public IScreen HostScreen { get; }
        public string UrlPathSegment { get; } = Guid.NewGuid().ToString().Substring(0, 5);

        public decimal NanoAmount { get; private set; }
        public decimal? Amount
        {
            get => _amount;
            set => this.RaiseAndSetIfChanged(ref _amount, value);
        }
        public string NanoAmountText { get; private set; }
        public string NanoAccount
        {
            get => _nanoAccount;
            set => this.RaiseAndSetIfChanged(ref _nanoAccount, value);
        }
        public bool IsFiatVisible { get; private set; }

        public bool IsUsdChecked { get; set; }
        public bool IsEurChecked { get; set; }
        public bool IsNanoChecked { get; set; }
        public bool FiatEnabled { get; private set; } = true;
        public string WarningMessage { get; private set; }

        public ReactiveCommand<Unit, Unit> Check => ReactiveCommand.Create(CurrencyChecked);

        public ReactiveCommand<Unit, IRoutableViewModel> Home => ReactiveCommand.CreateFromObservable(() => HostScreen.Router.NavigateAndReset.Execute(new HomeViewModel(HostScreen, true)));
        public ReactiveCommand<Unit, IRoutableViewModel> Request { get; }

        private PriceProvider _priceProvider;
        private decimal? _amount;
        private string _nanoAccount;

        public PaymentViewModel(IScreen screen)
        {
            HostScreen = screen;

            Request = ReactiveCommand.CreateFromObservable(
                () => HostScreen.Router.Navigate.Execute(new TransactionViewModel(screen, NanoAccount, Tools.ToRaw(NanoAmount))), 
                canExecute: this.IsValid());

            _priceProvider = Locator.Current.GetService<PriceProvider>();
            EnsureState(_priceProvider);

            var config = Locator.Current.GetService<Configuration>();
            _nanoAccount = config.NanoAccount;

            this.WhenAnyValue(x => x.Amount)
                .Throttle(TimeSpan.FromSeconds(.25))
                .Subscribe(x => Calculate());

            this.ValidationRule(vm => vm.NanoAccount, account => Tools.ValidateAccount(account), "Invalid account");
            this.ValidationRule(vm => vm.Amount, amount => amount.HasValue && amount.Value > 0, "Invalid amount");
        }

        private void EnsureState(PriceProvider priceProvider)
        {
            if (priceProvider.Initialized)
                IsUsdChecked = true;
            else
            {
                IsNanoChecked = true;
                FiatEnabled = false;
                WarningMessage = "Error retrieving Nano USD/EUR price.";
            }

            if (!priceProvider.UpToDate)
                WarningMessage = "Nano price might not be up to date.";

            CurrencyChecked();
        }

        public void Calculate()
        {
            if (!Amount.HasValue)
            {
                NanoAmount = 0;
                return;
            }

            if (IsNanoChecked)
                NanoAmount = Amount.Value;
            else
            {
                if (IsUsdChecked)
                {
                    NanoAmount = _priceProvider.UsdToNano(Amount.Value);
                    this.RaisePropertyChanged(nameof(NanoAmount));
                }

                if (IsEurChecked)
                {
                    NanoAmount = _priceProvider.EurToNano(Amount.Value);
                    this.RaisePropertyChanged(nameof(NanoAmount));
                }

                NanoAmountText = $"{NanoAmount} NANO";
                this.RaisePropertyChanged(nameof(NanoAmountText));
            }
        }

        private void CurrencyChecked()
        {
            IsFiatVisible = !IsNanoChecked;
            this.RaisePropertyChanged(nameof(IsFiatVisible));

            Calculate();
        }

    }
}
