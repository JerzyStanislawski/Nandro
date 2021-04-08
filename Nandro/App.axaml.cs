using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Nandro.ViewModels;
using Nandro.Views;
using System.Text.Json;

namespace Nandro
{
    public class App : Application
    {
        private MainWindowViewModel _mainWindowVM;

        public override void Initialize()
        {
            _mainWindowVM = new MainWindowViewModel();

            AvaloniaXamlLoader.Load(this);

            _mainWindowVM.DisplayQR();

            using var socket = new NanoSocket();
            if (socket.Subscribe("wss://socket.nanos.cc", "nano_34prihdxwz3u4ps8qjnn14p7ujyewkoxkwyxm3u665it8rg5rdqw84qrypzk"))
            {
                var response = socket.Listen();
                _mainWindowVM.Block = JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true});
            }
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
