using System;
using System.Threading;

namespace Nandro.Nano
{
    public interface INanoSocketClient : IDisposable
    {
        bool Subscribe(string url, string nanoAddress);
        NanoConfirmationResponse Listen(CancellationToken cancellationToken);
        void Close();
        bool Connected { get; }
    }
}