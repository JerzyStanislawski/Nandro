using Avalonia.Threading;
using Nandro.Models;
using Nandro.Nano;
using Nandro.NFC;
using Nandro.Providers;
using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;

namespace Nandro.ViewModels
{
    public class MainWindowViewModel : ReactiveObject, IScreen
    {
        public RoutingState Router { get; } = new RoutingState();

        private Currency _currency;
        public bool CurrencyPriceVisible { get; private set; }
        public string NanoCurrencyPrice { get; private set; } = "???";
        public string NanoUsdPrice { get; private set; } = "$???";
        public string NanoEurPrice { get; private set; } = "??? €";
        public string CurrencyChartName { get; private set; }
        public AccountInfo AccountInfo { get; private set; }
        public IEnumerable<TransactionEntry> LatestTransactions => AccountInfo?.LatestTransactions;
        public string BalanceText => $"{AccountInfo?.Balance.ToString("0.00")} NANO";
        public string PendingText => $"{AccountInfo?.Pending.ToString("0.00")} NANO";
        public string RepresentativeShortened => Tools.ShortenAccount(AccountInfo?.Representative);
        public string AccountShortened => Tools.ShortenAccount(AccountInfo?.Account);

        public ReactiveCommand<string, Unit> ViewAccount => ReactiveCommand.Create<string>(Tools.ViewAccountHistory);
        public ReactiveCommand<string, Unit> ViewTransactionBlock => ReactiveCommand.Create<string>(Tools.ViewTransaction);
        public ReactiveCommand<Unit, Unit> ViewUsdChart => ReactiveCommand.Create(() => Tools.ViewChart("usd"));
        public ReactiveCommand<Unit, Unit> ViewEurChart => ReactiveCommand.Create(() => Tools.ViewChart("eur"));
        public ReactiveCommand<Unit, Unit> ViewCurrencyChart => ReactiveCommand.Create(() => Tools.ViewChart(_currency.Code.ToLower()));
        public CombinedReactiveCommand<Unit, IObservable<IRoutableViewModel>> Home => ReactiveCommand.CreateCombined(new[] {
            ReactiveCommand.Create(BeforeHomeNav), ReactiveCommand.Create(() => Router.NavigateAndReset.Execute(new HomeViewModel(this, true))) });

        public string NfcDeviceName { get; private set; }
        public bool NfcDeviceConnected => NfcDeviceName != null;

        public EndpointTestResult ConnectionState { get; private set; }
        public string ConnectionStateDescription { get; private set; }

        private Configuration _config;
        private PriceProvider _priceProvider;
        private CurrencyProvider _currencyProvider;
        private MinuteTimer _priceTimer;
        private MinuteTimer _transactionsTimer;

        public MainWindowViewModel()
        {
            var homeViewModel = new HomeViewModel(this, false);
            Router.NavigateAndReset.Execute(homeViewModel);
                        
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(30));

            _config = Locator.Current.GetService<Configuration>();
            _priceProvider = Locator.Current.GetService<PriceProvider>();
            _currencyProvider = Locator.Current.GetService<CurrencyProvider>();
            Task.Run(() => _priceProvider.Initialize(), cancellationTokenSource.Token)
                .ContinueWith(task => Dispatcher.UIThread.InvokeAsync(() => homeViewModel.EnableRequests()))
                .ContinueWith(task => _priceTimer = new MinuteTimer(DisplayPrice));

            Task.Run(() => UpdateAccountInfo());
            _transactionsTimer = new MinuteTimer(UpdateLatestTransactions, false);

            InitNFCMonitor();
        }

        private void InitNFCMonitor()
        {
            var nfcMonitor = Locator.Current.GetService<NFCMonitor>();

            nfcMonitor.DeviceStatusChanged += NfcMonitor_DeviceStatusChanged;
            nfcMonitor.Start();

            var device = nfcMonitor.DetectDevice();
            if (device != null)
                NfcDeviceName = device.Name;
        }

        private void NfcMonitor_DeviceStatusChanged(object sender, DeviceEventArgs e)
        {
            if (e.Device == null)
                NfcDeviceName = null;
            else
                NfcDeviceName = e.Device.Name;

            this.RaisePropertyChanged(nameof(NfcDeviceName));
            this.RaisePropertyChanged(nameof(NfcDeviceConnected));
        }

        public void DisplayPrice()
        {
            _currency = _currencyProvider.DefaultCurrency;
            CurrencyPriceVisible = !_currencyProvider.IsDefaultCurrencyStandard;
            if (CurrencyPriceVisible)
            {
                NanoCurrencyPrice = $"{_priceProvider.CurrencyPrice} {_currency.Symbol}";
                CurrencyChartName = $"{_currency.Code} chart";
            }
            this.RaisePropertyChanged(nameof(NanoCurrencyPrice));
            this.RaisePropertyChanged(nameof(CurrencyChartName));
            this.RaisePropertyChanged(nameof(CurrencyPriceVisible));

            NanoUsdPrice = $"${_priceProvider.UsdPrice.ToString("0.00")}";
            this.RaisePropertyChanged(nameof(NanoUsdPrice));

            NanoEurPrice = $"{_priceProvider.EurPrice.ToString("0.00")} €";
            this.RaisePropertyChanged(nameof(NanoEurPrice));
        }

        public void UpdateAccountInfo()
        {
            if (String.IsNullOrEmpty(_config.NanoAccount))
                return;

            if (AccountInfo == null || AccountInfo.Account != _config.NanoAccount)
                AccountInfo = new AccountInfo(_config.NanoAccount);

            Task.Run(() => AccountInfo.GetAccountInfo())
                .ContinueWith(task => AccountInfoPropertyChanged())
                .ContinueWith(task => UpdateLatestTransactions());
        }

        private void UpdateLatestTransactions()
        {            
            AccountInfo?.UpdateTransactions();
            Dispatcher.UIThread.InvokeAsync(() => this.RaisePropertyChanged(nameof(LatestTransactions)));
            AccountInfoPropertyChanged();
        }

        private void AccountInfoPropertyChanged()
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                this.RaisePropertyChanged(nameof(AccountShortened));
                this.RaisePropertyChanged(nameof(RepresentativeShortened));
                this.RaisePropertyChanged(nameof(BalanceText));
                this.RaisePropertyChanged(nameof(PendingText));
                this.RaisePropertyChanged(nameof(AccountInfo));
            });
        }

        private void CheckConnections()
        {
            var tester = Locator.Current.GetService<NanoEndpointsTester>();
            ConnectionState = tester.TestState(_config);
            ConnectionStateDescription = ConnectionState == EndpointTestResult.Success ? "Online" : "Offline";

            Dispatcher.UIThread.InvokeAsync(() =>
            {
                this.RaisePropertyChanged(nameof(ConnectionState));
                this.RaisePropertyChanged(nameof(ConnectionStateDescription));
            });
        }

        private IObservable<IRoutableViewModel> BeforeHomeNav()
        {
            var transactionViewModel = Router.FindViewModelInStack<TransactionViewModel>();
            transactionViewModel?.Leave();

            return null;
        }
    }
}
