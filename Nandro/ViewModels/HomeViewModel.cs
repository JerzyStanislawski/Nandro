using ReactiveUI;
using System;
using System.Reactive;

namespace Nandro.ViewModels
{
    public class HomeViewModel : ReactiveObject, IRoutableViewModel
    {
        public IScreen HostScreen { get; }

        public string UrlPathSegment { get; } = Guid.NewGuid().ToString().Substring(0, 5);

        public ReactiveCommand<Unit, IRoutableViewModel> RequestPayment { get; private set; }
        public ReactiveCommand<Unit, IRoutableViewModel> Settings { get; private set; }

        public HomeViewModel(IScreen hostScreen, bool enableRequests)
        {
            HostScreen = hostScreen;

            Settings = ReactiveCommand.CreateFromObservable(
                () => HostScreen.Router.Navigate.Execute(new SettingsViewModel(HostScreen)));

            if (enableRequests)
                EnableRequests();
        }

        public void EnableRequests()
        {
            RequestPayment = ReactiveCommand.CreateFromObservable(
                () => HostScreen.Router.Navigate.Execute(new PaymentViewModel(HostScreen)));
            this.RaisePropertyChanged(nameof(RequestPayment));
        }
    }
}
