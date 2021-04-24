using Avalonia.Threading;
using DotNano.Shared.DataTypes;
using Nandro.Nano;
using Nandro.NFC;
using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;

namespace Nandro.ViewModels
{
    public class MainWindowViewModel : ReactiveObject, IScreen
    {
        public RoutingState Router { get; } = new RoutingState();

        public string NanoUsdPrice { get; private set; } = "1 NANO = ??? USD";
        public string NanoEurPrice { get; private set; } = "1 NANO = ??? EUR";
        public List<TransactionEntry> LatestTransactions { get; private set; }
        public string Account => Tools.ShortenAccount(_config.NanoAccount);
        public bool AccountProvided => !String.IsNullOrEmpty(_config.NanoAccount);

        public ReactiveCommand<Unit, Unit> ViewAccount => ReactiveCommand.Create(() => Tools.ViewAccountHistory(_config.NanoAccount));
        public ReactiveCommand<string, Unit> ViewTransactionBlock => ReactiveCommand.Create<string>(Tools.ViewTransaction);

        public string NfcDeviceName { get; private set; }
        public bool NfcDeviceConnected => NfcDeviceName != null;

        private Configuration _config;
        private PriceProvider _priceProvider;
        private Timer _priceTimer;
        private Timer _transactionsTimer;

        public MainWindowViewModel()
        {
            var homeViewModel = new HomeViewModel(this, false);
            Router.NavigateAndReset.Execute(homeViewModel);

            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(30));

            _config = Locator.Current.GetService<Configuration>();
            _priceProvider = Locator.Current.GetService<PriceProvider>();
            Task.Run(() => _priceProvider.Initialize(), cancellationTokenSource.Token)
                .ContinueWith(task => Dispatcher.UIThread.InvokeAsync(() => homeViewModel.EnableRequests()))
                .ContinueWith(task => _priceTimer = new Timer(state => DisplayPrice(state), null, 0, 60 * 1000));

            _transactionsTimer = new Timer(state => UpdateLatestTransactions(), null, 0, 60 * 1000);

            InitNFCMonitor();
        }

        private void InitNFCMonitor()
        {
            var nfcMonitor = Locator.Current.GetService<NFCMonitor>();
            var device = nfcMonitor.DetectDevice();

            if (device != null)
                NfcDeviceName = device.Name;

            nfcMonitor.DeviceStatusChanged += NfcMonitor_DeviceStatusChanged;
            nfcMonitor.Start();
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

        private void DisplayPrice(object _)
        {
            NanoUsdPrice = $"1 NANO = {_priceProvider.UsdPrice} USD";
            this.RaisePropertyChanged(nameof(NanoUsdPrice));

            NanoEurPrice = $"1 NANO = {_priceProvider.EurPrice} EUR";
            this.RaisePropertyChanged(nameof(NanoEurPrice));
        }

        public void UpdateLatestTransactions()
        {
            if (!String.IsNullOrEmpty(_config.NanoAccount))
            {
                var nanoNodeClient = Locator.Current.GetService<INanoClient>();
                var transactions = nanoNodeClient.GetLatestTransactions(_config.NanoAccount, 10);

                LatestTransactions = transactions.History.Select(x => new TransactionEntry(x.Hash, x.Amount, x.Type, DateTimeOffset.FromUnixTimeSeconds(x.LocalTimestamp).UtcDateTime)).ToList();
                Dispatcher.UIThread.InvokeAsync(() => this.RaisePropertyChanged(nameof(LatestTransactions)));
            }
        }

        public void UpdateView()
        {
            this.RaisePropertyChanged(nameof(Account));
            this.RaisePropertyChanged(nameof(AccountProvided));

            Task.Run(() => UpdateLatestTransactions());
        }

    }

    public class TransactionEntry
    {
        public string Hash { get; }
        public decimal Amount { get; }
        public string Type { get; }
        public DateTime TimeStamp { get; }
        public string AmountText => Amount < 0.0001m ? "< 0.0001" : Amount.ToString("0.0000");
        public string RelativeTime => TimeHelpers.RelativeTime(TimeStamp);

        public TransactionEntry(HexKey64 hash, BigInteger amount, string type, DateTime timeStamp)
        {
            Hash = hash.HexKeyString;
            Amount = Tools.ToNano(amount);
            Type = type;
            TimeStamp = timeStamp;
        }
    }
}
