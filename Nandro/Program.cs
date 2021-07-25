using Avalonia;
using Avalonia.ReactiveUI;
using Nandro.Data;
using Nandro.Nano;
using Nandro.NFC;
using Nandro.TransactionMonitors;
using Nandro.ViewModels;
using Nandro.Views;
using NdefLibrary.Ndef;
using ReactiveUI;
using Splat;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Nandro.Tests")]
namespace Nandro
{
    class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args)
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
        {
            RegisterServices();

            return AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .UseReactiveUI();
        }

        private static void RegisterServices()
        {
            var dbContext = new NandroDbContext();
            var config = dbContext.Configuration.Single();

            Locator.CurrentMutable.RegisterConstant(config);
            Locator.CurrentMutable.Register(() => new HomeView(), typeof(IViewFor<HomeViewModel>));
            Locator.CurrentMutable.Register(() => new PaymentView(), typeof(IViewFor<PaymentViewModel>));
            Locator.CurrentMutable.Register(() => new ProductsView(), typeof(IViewFor<ProductsViewModel>));
            Locator.CurrentMutable.Register(() => new SettingsView(), typeof(IViewFor<SettingsViewModel>));
            Locator.CurrentMutable.Register(() => new TransactionView(), typeof(IViewFor<TransactionViewModel>));
            Locator.CurrentMutable.Register(() => new TransactionResultView(), typeof(IViewFor<TransactionResultViewModel>));

            Locator.CurrentMutable.RegisterLazySingleton(() => new PriceProvider());
            Locator.CurrentMutable.RegisterLazySingleton(() => new NanoEndpointsTester());

            Locator.CurrentMutable.RegisterLazySingleton(() => dbContext);

            var apiClient = string.IsNullOrEmpty(config.NodeUri) ? new NanoNodeClient(config.PublicNanoApiUri) : new NanoNodeClient(config.NodeUri);
            Locator.CurrentMutable.RegisterLazySingleton<INanoClient>(() => apiClient);

            Locator.CurrentMutable.Register(() =>
            {
                var publicApiClient = new NanoNodeClient(config.PublicNanoApiUri);
                var nanoSocket = new NanoSocketClient(new SocketWrapper(), config);
                if (string.IsNullOrEmpty(config.NodeUri))
                    return new TransactionMonitor(new SocketTransactionMonitor(nanoSocket, config), new RpcTransactionMonitor(publicApiClient, config), null, config);
                else
                {
                    var localApiClient = new NanoNodeClient(config.NodeUri);
                    return new TransactionMonitor(new SocketTransactionMonitor(nanoSocket, config), new RpcTransactionMonitor(publicApiClient, config), new RpcTransactionMonitor(localApiClient, config), config);
                }
            });

            Locator.CurrentMutable.RegisterLazySingleton(() => new NFCMonitor());
        }
    }
}
