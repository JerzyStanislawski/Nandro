using QRCoder;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace Nandro.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public string Greeting => "Welcome to Avalonia!";

        public Avalonia.Media.Imaging.Bitmap Bitmap { get; set; }

        public string Block { get; set; }

        public void DisplayQR()
        {
            var data = "nano:nano_3wm37qz19zhei7nzscjcopbrbnnachs4p1gnwo5oroi3qonw6inwgoeuufdp?amount=150000000000000000000000000000";

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
