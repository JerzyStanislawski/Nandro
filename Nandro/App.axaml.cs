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
