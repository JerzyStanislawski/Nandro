using QRCoder;
using System.Drawing.Imaging;
using System.IO;
using System.Numerics;
using System.Text;

namespace Nandro.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public string Greeting => "Welcome to Avalonia!";

        public Avalonia.Media.Imaging.Bitmap Bitmap { get; set; }

        public string Block { get; set; }

        public void DisplayQR(string nanoAccount, BigInteger amount)
        {
            var data = $"nano:{nanoAccount}?amount={amount}";

            using var qrGenerator = new QRCodeGenerator();
            var qrData = qrGenerator.CreateQrCode(Encoding.UTF8.GetBytes(data), QRCodeGenerator.ECCLevel.H);
            var qrCode = new QRCode(qrData);
            var bitmap = qrCode.GetGraphic(20);

            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                Bitmap = new Avalonia.Media.Imaging.Bitmap(memory);
            }
        }
    }
}
