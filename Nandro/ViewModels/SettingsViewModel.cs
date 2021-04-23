using Avalonia.Threading;
using Nandro.Nano;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;
using Splat;
using System;
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

        private Configuration _config;

        public ReactiveCommand<Unit, Unit> GoBack => HostScreen.Router.NavigateBack;
        public CombinedReactiveCommand<Unit, Unit> Save => ReactiveCommand.CreateCombined(new[] { ReactiveCommand.Create(Persist), HostScreen.Router.NavigateBack }, canExecute: this.IsValid());

        public ReactiveCommand<Unit, Unit> TestNodeUri => ReactiveCommand.Create(TestNode);
        public ReactiveCommand<Unit, Unit> TestSocketUri => ReactiveCommand.Create(TestSocket);

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

            _config.Persist();
        }

        private void TestSocket()
        {
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(30));

            Task.Run(() =>
            {
                if (!String.IsNullOrEmpty(NodeSocketUri))
                {
                    var tester = Locator.Current.GetService<NanoEndpointsTester>();
                    return tester.TestSocket(NodeSocketUri);
                }
                return false;
            });
            //.ContinueWith(task => Dispatcher.UIThread.InvokeAsync())
        }

        private void TestNode()
        {
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(30));

            Task.Run(() =>
            {
                if (!String.IsNullOrEmpty(NodeUri))
                {
                    var tester = Locator.Current.GetService<NanoEndpointsTester>();
                    tester.TestNode(NodeUri);
                }
            });
            //.ContinueWith(task => Dispatcher.UIThread.InvokeAsync())
        }
    }
}
