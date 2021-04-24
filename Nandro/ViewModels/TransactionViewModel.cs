using Avalonia;
using Avalonia.Threading;
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
        public string CountDown { get; set; }

        private CancellationTokenSource _cancellation = new CancellationTokenSource();

        DispatcherTimer _timer;
        private readonly string _nanoAccount;

        public TransactionViewModel(IScreen screen, string nanoAccount, BigInteger amount)
        {
            HostScreen = screen;
            _nanoAccount = nanoAccount;

            DisplayQR(nanoAccount, amount);

            Task.Run(() =>
            {
                using var transactionMonitor = Locator.Current.GetService<TransactionMonitor>();
                var result = transactionMonitor.Verify(nanoAccount, amount, out var blockHash, _cancellation);
                
                MoveForward(blockHash, result);
            });

            Config = Locator.Current.GetService<Configuration>();
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
            });

            _timer.Start();
        }

        private void DisplayQR(string nanoAccount, BigInteger amount)
        {
            var data = $"nano:{nanoAccount}?amount={amount}";

            using var qrGenerator = new QRCodeGenerator();
            var qrData = qrGenerator.CreateQrCode(Encoding.UTF8.GetBytes(data), QRCodeGenerator.ECCLevel.H);
            var qrCode = new QRCode(qrData);
            var bitmap = qrCode.GetGraphic(20);

            using var memoryStream = new MemoryStream();
            bitmap.Save(memoryStream, ImageFormat.Png);
            memoryStream.Position = 0;

            Bitmap = new Avalonia.Media.Imaging.Bitmap(memoryStream);
        }

        private void Leave()
        {
            try
            {
                _timer.Stop();
                _cancellation.Cancel();
            }
            catch
            {
            }
        }

        private void MoveForward(string blockHash, bool success)
        {
            _timer.Stop();
            Dispatcher.UIThread.InvokeAsync(() => HostScreen.Router.Navigate.Execute(
                new TransactionResultViewModel(HostScreen, blockHash, _nanoAccount, success)));
        }
    }
}
