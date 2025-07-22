using System.IO;
using POS_ModernUI.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace POS_ModernUI.Services.ImageServices;
public class ImageCompressor
{
    public async Task<string> SaveCompressedAsync(string sourcePath, string destination,string name, int maxWidth = 256)
    {
        if (!File.Exists(sourcePath))
            throw new FileNotFoundException("Source image not found", sourcePath);

        try
        {
            var image = await Image.LoadAsync(sourcePath);

            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = SixLabors.ImageSharp.Processing.ResizeMode.Max,
                Size = new SixLabors.ImageSharp.Size(maxWidth, 0)
            }));

            var outputPath = Path.Combine(destination, name + ".png");
            await image.SaveAsPngAsync(outputPath, new PngEncoder
            {
                CompressionLevel = PngCompressionLevel.Level6
            });

            return outputPath;
        }
        catch (Exception ex)
        {
            var msg = new Wpf.Ui.Controls.MessageBox();
            msg.ShowMessage(ex.Message, "تحذيز خطأ فى النظام");

            return null;
        }
    }
}
