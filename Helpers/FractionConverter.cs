using System.Globalization;
using System.Windows.Data;

namespace POS_ModernUI.Helpers;
public class FractionConverter: IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        double originalWidth = (double)value;
        double fraction = System.Convert.ToDouble(parameter, CultureInfo.InvariantCulture);
        return originalWidth * fraction;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
