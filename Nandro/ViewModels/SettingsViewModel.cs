using Avalonia.Threading;
using Nandro.Data;
using Nandro.Nano;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;
using Splat;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;

namespace Nandro.ViewModels
{
    public class SettingsViewModel : ReactiveValidationObject, IRoutableViewModel
    {
        public IScreen HostScreen { get; }
        public string UrlPathSegment { get; } = Guid.NewGuid().ToString().Substring(0, 5);

        string _nanoAccount;
        public string NanoAccount
        {
            get => _nanoAccount;
            set => this.RaiseAndSetIfChanged(ref _nanoAccount, value);
        }
        private string _nodeUri;
        public string NodeUri
        {
            get => _nodeUri;
            set => this.RaiseAndSetIfChanged(ref _nodeUri, value);
        }
        private string _nodeSocketUri;
        public string NodeSocketUri
        {
            get => _nodeSocketUri;
            set => _nodeSocketUri = this.RaiseAndSetIfChanged(ref _nodeSocketUri, value);
        }
        private bool _ownNode;
        public bool OwnNode
        {
            get => _ownNode;
            set => _ownNode = this.RaiseAndSetIfChanged(ref _ownNode, value);
        }

        private string _socketTestResultDescription;
        public string SocketTestResultDescription
        {
            get => _socketTestResultDescription;
            private set => this.RaiseAndSetIfChanged(ref _socketTestResultDescription, value);
        }
        private string _rpcTestResultDescription;
        public string RpcTestResultDescription
        {
            get => _rpcTestResultDescription;
            private set => this.RaiseAndSetIfChanged(ref _rpcTestResultDescription, value);
        }
        private EndpointTestResult _socketTestResult;
        public EndpointTestResult SocketTestResult 
        { 
            get => _socketTestResult;
            private set => this.RaiseAndSetIfChanged(ref _socketTestResult, value); 
        }
        private EndpointTestResult _rpcTestResult;
        public EndpointTestResult RpcTestResult
        {
            get => _rpcTestResult;
            private set => this.RaiseAndSetIfChanged(ref _rpcTestResult, value);
        }

        private Configuration _config;
        private NandroDbContext _dbContext;

        public ReactiveCommand<Unit, Unit> GoBack => HostScreen.Router.NavigateBack;
        public CombinedReactiveCommand<Unit, Unit> Save => ReactiveCommand.CreateCombined(new[] { ReactiveCommand.Create(Persist), ReactiveCommand.Create(UpdateMainScreen), HostScreen.Router.NavigateBack }, canExecute: this.IsValid());

        public ReactiveCommand<Unit, Unit> TestNodeUri => ReactiveCommand.Create(TestNode);
        public ReactiveCommand<Unit, Unit> TestSocketUri => ReactiveCommand.Create(TestSocket);
        public ReactiveCommand<string, Unit> GoToLink => ReactiveCommand.Create<string>(uri => Process.Start("explorer", uri));

        public SettingsViewModel(IScreen hostScreen)
        {
            HostScreen = hostScreen;

            LoadConfig();

            this.ValidationRule(vm => vm.NanoAccount,
                account => String.IsNullOrEmpty(account) || Tools.ValidateAccount(account),
                "Invalid account");

            var ownNodeObservable = this.WhenAnyValue(vm => vm.NodeUri, vm => vm.NodeSocketUri, vm => vm.OwnNode,
                (nodeUri, socketUri, ownNode) => ownNode ? !String.IsNullOrEmpty(nodeUri) || !String.IsNullOrEmpty(socketUri) : true);
            this.ValidationRule(vm => vm.NodeUri, ownNodeObservable, "Provide Node details");
            this.ValidationRule(vm => vm.NodeSocketUri, ownNodeObservable, "Provide Node details");
        }

        private void LoadConfig()
        {
            _dbContext = Locator.Current.GetService<NandroDbContext>();
            _config = Locator.Current.GetService<Configuration>();

            NanoAccount = _config.NanoAccount;
            NodeUri = _config.NodeUri;
            NodeSocketUri = _config.NodeSocketUri;
            OwnNode = _config.OwnNode;
        }

        private void Persist()
        {
            _config.NanoAccount = NanoAccount;
            _config.OwnNode = OwnNode;

            if (OwnNode)
            {
                _config.NodeUri = NodeUri;
                _config.NodeSocketUri = NodeSocketUri;
            }
            else
            {
                _config.NodeUri = String.Empty;
                _config.NodeSocketUri = String.Empty;
            }

            //_dbContext.Configuration.Remove()
            _dbContext.SaveChanges();
        }

        private void TestSocket()
        {
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(30));

            SocketTestResultDescription = "Testing socket...";
            SocketTestResult = EndpointTestResult.Pending;

            Task.Run(() =>
            {
                if (!String.IsNullOrEmpty(NodeSocketUri))
                {
                    var tester = Locator.Current.GetService<NanoEndpointsTester>();
                    return tester.TestSocket(NodeSocketUri, out _socketTestResultDescription);
                }
                return false;
            }, cancellationTokenSource.Token)
            .ContinueWith(task => Dispatcher.UIThread.InvokeAsync(() =>
            {
                SocketTestResult = task.IsCompletedSuccessfully && task.Result ? EndpointTestResult.Success : EndpointTestResult.Fail;

                if (task.IsCompletedSuccessfully && task.Result)
                    SocketTestResultDescription = "Socket connection successful";
                else
                    this.RaisePropertyChanged(nameof(SocketTestResultDescription));
            }));
        }

        private void TestNode()
        {
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(30));

            RpcTestResultDescription = "Testing connection...";
            RpcTestResult = EndpointTestResult.Pending;

            Task.Run(() =>
            {
                if (!String.IsNullOrEmpty(NodeUri))
                {
                    var tester = Locator.Current.GetService<NanoEndpointsTester>();
                    return tester.TestNode(NodeUri, out _rpcTestResultDescription);
                }
                return false;
            }, cancellationTokenSource.Token)
            .ContinueWith(task => Dispatcher.UIThread.InvokeAsync(() =>
            {
                RpcTestResult = task.IsCompletedSuccessfully && task.Result ? EndpointTestResult.Success : EndpointTestResult.Fail;

                if (task.IsCompletedSuccessfully && task.Result)
                    RpcTestResultDescription = "Node RPC test successful";
                else
                    this.RaisePropertyChanged(nameof(RpcTestResultDescription));
            }));
        }

        private void UpdateMainScreen()
        {
            ((MainWindowViewModel)HostScreen).UpdateAccountInfo();
        }
    }
}
