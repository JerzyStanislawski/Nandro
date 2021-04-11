namespace Nandro.Nano
{
    internal interface INanoSocketClient
    {
        bool Subscribe(string url, string nanoAddress);
        NanoConfirmationResponse Listen();
        void Close();
        bool Connected { get; }
    }
}