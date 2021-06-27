using Avalonia;
using Avalonia.Threading;
using Nandro.Nano;
using Nandro.NFC;
using Nandro.TransactionMonitors;
using QRCoder;
using ReactiveUI;
using Splat;
using System;
using System.Drawing.Imaging;
using System.IO;
using System.Numerics;
using System.Reactive;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nandro.ViewModels
{
    public class TransactionViewModel : ReactiveObject, IRoutableViewModel
    {
        public IScreen HostScreen { get; }
        public string UrlPathSegment { get; } = Guid.NewGuid().ToString().Substring(0, 5);

        public CombinedReactiveCommand<Unit, Unit> GoBack => ReactiveCommand.CreateCombined(
            new[] { ReactiveCommand.Create(Leave), HostScreen.Router.NavigateBack });

        public Avalonia.Media.Imaging.Bitmap Bitmap { get; set; }

        public Configuration Config { get; }
        private NFCMonitor _nfcMonitor;

        public string CountDown { get; set; }
        public int Progress { get; set; }

        private CancellationTokenSource _verificationCancellation;
        private CancellationTokenSource _nfcCancellation;

        DispatcherTimer _timer;
        private readonly string _nanoAccount;
        private readonly BigInteger _amount;

        public TransactionViewModel(IScreen screen, string nanoAccount, BigInteger amount)
        {
            HostScreen = screen;
            _nanoAccount = nanoAccount;
            _amount = amount;

            Config = Locator.Current.GetService<Configuration>();
            _nfcMonitor = Locator.Current.GetService<NFCMonitor>();

            DisplayQR(nanoAccount, amount);

            Initialize();
        }

        public void Initialize()
        {
            _verificationCancellation = new CancellationTokenSource();
            _nfcCancellation = new CancellationTokenSource();

            Task.Run(() => _nfcMonitor.Device?.Transmit(_nanoAccount, _amount, _nfcCancellation.Token), _nfcCancellation.Token);
            Task.Run(() =>
            {
                using var transactionMonitor = Locator.Current.GetService<TransactionMonitor>();
                var result = transactionMonitor.Verify(_nanoAccount, _amount, out var blockHash, _verificationCancellation);

                if (!_verificationCancellation.IsCancellationRequested)
                {
                    MoveForward(blockHash, result);
                }
            }, _verificationCancellation.Token);

            Progress = 100;
            StartTimer();
        }

        private void StartTimer()
        {
            var time = TimeSpan.FromSeconds(Config.TransactionTimeoutSec);
            _timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
            {
                CountDown = time.ToString("mm':'ss");
                this.RaisePropertyChanged(nameof(CountDown));

                if (time <= TimeSpan.Zero)
                {
                    MoveForward(null, false);
                }
                time = time.Add(TimeSpan.FromSeconds(-1));

                Progress = (int)time.TotalSeconds * 100 / Config.TransactionTimeoutSec;
                this.RaisePropertyChanged(nameof(Progress));
            });

            _timer.Start();
        }

        private void DisplayQR(string nanoAccount, BigInteger amount)
        {
            var data = Tools.GenerateNanoUri(nanoAccount, amount);

            using var qrGenerator = new QRCodeGenerator();
            var qrData = qrGenerator.CreateQrCode(Encoding.UTF8.GetBytes(data), QRCodeGenerator.ECCLevel.H);
            var qrCode = new QRCode(qrData);
            var bitmap = qrCode.GetGraphic(20);

            using var memoryStream = new MemoryStream();
            bitmap.Save(memoryStream, ImageFormat.Png);
            memoryStream.Position = 0;

            Bitmap = new Avalonia.Media.Imaging.Bitmap(memoryStream);
        }

        public void Leave()
        {
            try
            {
                _timer.Stop();

                if (!_verificationCancellation.IsCancellationRequested)
                    _verificationCancellation?.Cancel();
                if (!_nfcCancellation.IsCancellationRequested)
                    _nfcCancellation?.Cancel();
            }
            catch
            {
            }
        }

        private void MoveForward(string blockHash, bool success)
        {
            _timer.Stop();
            _nfcCancellation.Cancel();

            Dispatcher.UIThread.InvokeAsync(() => HostScreen.Router.Navigate.Execute(
                new TransactionResultViewModel(HostScreen, blockHash, _nanoAccount, success)));
        }
    }
}
