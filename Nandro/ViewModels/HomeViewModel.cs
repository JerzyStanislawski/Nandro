using Nandro.Data;
using ReactiveUI;
using Splat;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace Nandro.ViewModels
{
    public class HomeViewModel : ReactiveObject, IRoutableViewModel
    {
        public IScreen HostScreen { get; }

        public string UrlPathSegment { get; } = Guid.NewGuid().ToString().Substring(0, 5);

        public ReactiveCommand<Unit, IRoutableViewModel> NewCart { get; private set; }
        public ReactiveCommand<Unit, IRoutableViewModel> RequestPayment { get; private set; }
        public ReactiveCommand<Unit, IRoutableViewModel> Settings { get; private set; }
        public ReactiveCommand<Unit, IRoutableViewModel> Products { get; private set; }

        public HomeViewModel(IScreen hostScreen, bool enableRequests)
        {
            HostScreen = hostScreen;

            Settings = ReactiveCommand.CreateFromObservable(
                () => HostScreen.Router.Navigate.Execute(new SettingsViewModel(HostScreen)));

            Products = ReactiveCommand.CreateFromObservable(
                () => HostScreen.Router.Navigate.Execute(new ProductsViewModel(HostScreen)));

            if (enableRequests)
                EnableRequests();
        }

        public void EnableRequests()
        {
            var dbContext = Locator.Current.GetService<NandroDbContext>();
           
            NewCart = ReactiveCommand.CreateFromObservable(
                () => HostScreen.Router.Navigate.Execute(new CartViewModel(HostScreen)),
                canExecute: Observable.Return(dbContext.Products.Any()));
            this.RaisePropertyChanged(nameof(NewCart));

            RequestPayment = ReactiveCommand.CreateFromObservable(
                () => HostScreen.Router.Navigate.Execute(new PaymentViewModel(HostScreen)));
            this.RaisePropertyChanged(nameof(RequestPayment));
        }
    }
}
