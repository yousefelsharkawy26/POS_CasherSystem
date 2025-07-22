using System.IO;
using System.Windows.Data;
using System.Globalization;

namespace POS_ModernUI.Helpers;
public class DbImageToPhysicalPathConverter: IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string)
        {
            // Convert byte array to base64 string
            var path = $"{Path.GetDirectoryName(AppContext.BaseDirectory)}{value.ToString()}";
            if (File.Exists(path))
            {
                return path;
            }
            return $"{Directory.GetCurrentDirectory()}\\Images\\DefaultProduct.png";
        }

        return $"{Directory.GetCurrentDirectory()}\\Images\\DefaultProduct.png"; // or return a default image path
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }
}
