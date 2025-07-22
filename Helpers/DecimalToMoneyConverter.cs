using System.Globalization;
using System.Windows.Data;

namespace POS_ModernUI.Helpers;
public class DecimalToMoneyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is decimal decimalValue)
        {
            // Convert the decimal value to a formatted string representing money
            return decimalValue.ToString("C", culture);
        }
        var stringValue = 0;

        return stringValue.ToString("C", culture); // Return "0" if the value is not a decimal
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
