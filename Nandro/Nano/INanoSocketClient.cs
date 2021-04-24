using System;
using System.Threading;

namespace Nandro.Nano
{
    public interface INanoSocketClient : IDisposable
    {
        bool Subscribe(string url, string nanoAddress, out string error);
        NanoConfirmationResponse Listen(CancellationToken cancellationToken);
        void Close();
        bool Connected { get; }
    }
}