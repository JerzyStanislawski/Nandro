using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Nandro.ViewModels;
using Nandro.Views;
using System.Numerics;
using System.Threading.Tasks;

namespace Nandro
{
    public class App : Application
    {
        private MainWindowViewModel _mainWindowVM;

        public override void Initialize()
        {
            _mainWindowVM = new MainWindowViewModel();

            AvaloniaXamlLoader.Load(this);

            var config = new Configuration();
            config.NanoSocketUri = "wss://socket2.nanos.cc";

            var nanoAccount = "nano_1iuz18n4g4wfp9gf7p1s8qkygxw7wx9qfjq6a9aq68uyrdnningdcjontgar";
            var amount = BigInteger.Parse("100000000000000000000000000");
            _mainWindowVM.DisplayQR(nanoAccount, amount);

            Task.Run(() =>
            {
                using var apiClient = new NanoApiClient("https://proxy.nanos.cc/proxy");
                using var nanoSocket = new NanoSocket();
                var monitor = new TransactionMonitor(nanoSocket, apiClient, config);

                var result = false;
                var (frontier, pendingTxs, connected) = monitor.Prepare(nanoAccount);
                if (connected)
                    result = monitor.VerifyWithSocket(nanoAccount, amount);
                else
                    result = monitor.VerifyWithNanoClient(nanoAccount, amount, frontier, pendingTxs?.Keys);
            });


            //var response = apiClient.GetLatestTransaction("nano_34prihdxwz3u4ps8qjnn14p7ujyewkoxkwyxm3u665it8rg5rdqw84qrypzk");

            //using var socket = new NanoSocket();
            //if (socket.Subscribe("wss://socket.nanos.cc", "nano_34prihdxwz3u4ps8qjnn14p7ujyewkoxkwyxm3u665it8rg5rdqw84qrypzk"))
            //{
            //    var response = socket.Listen();
            //    _mainWindowVM.Block = JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true});
            //}
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = _mainWindowVM,
                };
            }

            base.OnFrameworkInitializationCompleted();
        }        
    }
}
