using Nandro.Nano;
using ReactiveUI;
using System;
using System.Reactive;

namespace Nandro.ViewModels
{
    public class TransactionResultViewModel : ReactiveObject, IRoutableViewModel
    {
        public IScreen HostScreen { get; }
        public string UrlPathSegment { get; } = Guid.NewGuid().ToString().Substring(0, 5);

        public Avalonia.Media.Imaging.Bitmap Bitmap { get; set; }
        public string NanoExplorerButtonText { get; set; }

        public ReactiveCommand<Unit, Unit> OpenNanoExplorer => ReactiveCommand.Create(ViewInExplorer);

        public ReactiveCommand<Unit, IRoutableViewModel> Home => ReactiveCommand.CreateFromObservable(() => HostScreen.Router.NavigateAndReset.Execute(new HomeViewModel(HostScreen, true)));
        public ReactiveCommand<Unit, IRoutableViewModel> NewTransaction { get; }

        private readonly string _blockHash;
        private readonly string _nanoAccount;
        private readonly bool _success;

        public TransactionResultViewModel(IScreen screen, string blockHash, string nanoAccount, bool success)
        {
            HostScreen = screen;
            _nanoAccount = nanoAccount;
            _blockHash = blockHash;
            _success = success;

            Bitmap = new Avalonia.Media.Imaging.Bitmap(success ? ".\\Assets\\success.png" : ".\\Assets\\fail.png");

            NewTransaction = ReactiveCommand.CreateFromObservable(
                () => HostScreen.Router.Navigate.Execute(new PaymentViewModel(HostScreen)));

            NanoExplorerButtonText = success ? "Transaction detais" : "Account history";

            if (success)
                ((MainWindowViewModel)screen).UpdateView();
        }

        private void ViewInExplorer()
        {
            if (_success)
                Tools.ViewTransaction(_blockHash);
            else
                Tools.ViewAccountHistory(_nanoAccount);
        }
    }
}
