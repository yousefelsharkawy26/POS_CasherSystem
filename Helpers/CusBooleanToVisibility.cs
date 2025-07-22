using System.Globalization;
using System.Windows.Data;

namespace POS_ModernUI.Helpers;
public class CusBooleanToVisibility : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool booleanValue)
        {
            return !booleanValue ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
        }
        return System.Windows.Visibility.Hidden;
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is System.Windows.Visibility visibility)
        {
            return visibility == System.Windows.Visibility.Visible;
        }
        return false;
    }
}
