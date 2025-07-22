using QRCoder;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using BarcodeStandard;

namespace POS_ModernUI.Services;
public class IPQRService
{
    public BitmapImage GenerateQRCode(string ip, int port)
    {
        string json = $"{{\"ip\":\"{ip}\",\"port\":{port}}}";


        QRCodeGenerator qrGenerator = new QRCodeGenerator();
        QRCodeData qrCodeData = qrGenerator.CreateQrCode(json, QRCodeGenerator.ECCLevel.Q);
        QRCode qrCode = new QRCode(qrCodeData);
        Bitmap qrCodeImage = qrCode.GetGraphic(20);

        using (MemoryStream ms = new MemoryStream())
        {
            qrCodeImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = ms;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            return bitmapImage;
        }
    }

    public BitmapImage GenerateBarcode(string text)
    {
        Barcode barcode = new Barcode();
        barcode.IncludeLabel = true;
        barcode.Encode(BarcodeStandard.Type.Code128, text);
        using (MemoryStream ms = new MemoryStream())
        {
            barcode.SaveImage(ms, SaveTypes.Png);
            ms.Position = 0;
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = ms;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            return bitmapImage;
        }
    }
}
