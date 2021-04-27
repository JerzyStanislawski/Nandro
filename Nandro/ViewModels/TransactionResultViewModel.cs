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
        public string ActionText { get; set; }
        public string TransactionResultText { get; set; }
        public bool Failed => !_success;

        public ReactiveCommand<Unit, Unit> OpenNanoExplorer => ReactiveCommand.Create(ViewInExplorer);

        public CombinedReactiveCommand<Unit, Unit> Retry => ReactiveCommand.CreateCombined(
            new[] { ReactiveCommand.Create(PrepareRetry), HostScreen.Router.NavigateBack });

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

            TransactionResultText = success ? "Success! " : "Transaction not detected! ";
            ActionText = success ? " to view transaction in explorer." : " to check account history.";

            if (success)
                ((MainWindowViewModel)screen).UpdateAccountInfo();
        }

        private void ViewInExplorer()
        {
            if (_success)
                Tools.ViewTransaction(_blockHash);
            else
                Tools.ViewAccountHistory(_nanoAccount);
        }

        private void PrepareRetry()
        {
            var transactionVm = HostScreen.Router.FindViewModelInStack<TransactionViewModel>();
            transactionVm?.Initialize();
        }
    }
}
