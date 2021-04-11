using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Nandro.Nano;
using Nandro.TransactionMonitors;
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
            config.NanoSocketUri = "wss://socket.nanos.cc";
            config.TransactionTimeoutSec = 60;

            var nanoAccount = "nano_3wm37qz19zhei7nzscjcopbrbnnachs4p1gnwo5oroi3qonw6inwgoeuufdp";
            var amount = BigInteger.Parse("100000000000000000000000000");
            _mainWindowVM.DisplayQR(nanoAccount, amount);

            Task.Run(() =>
            {
                using var apiClient = new NanoNodeClient("https://proxy.nanos.cc/proxy");
                using var nanoSocket = new NanoSocketClient(new SocketWrapper(), config);
                var monitor = new TransactionMonitor(new SocketTransactionMonitor(nanoSocket, config), new RpcTransactionMonitor(apiClient, config));
                monitor.Verify(nanoAccount, amount);
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
