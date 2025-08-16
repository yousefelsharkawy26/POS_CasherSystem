using System.Globalization;
using System.Windows.Data;

namespace POS_ModernUI.Helpers;

public class PercentageConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is decimal decimalValue)
        {
            return $"نسبة المدفوع {decimalValue.ToString("0")}%";
        }
        return "0%";
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException("ConvertBack is not implemented.");
    }
}
