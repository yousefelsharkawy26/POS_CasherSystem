using POS_ModernUI.Models;
using POS_ModernUI.ViewModels.Windows;
using System.Globalization;
using System.Windows.Data;

namespace POS_ModernUI.Helpers;

public class ProductToQuantityWithUnitConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Product product)
        {
            var quantity = product.QuantityInStock;
            var productUnit = product.UnitShares.FirstOrDefault(u => u.QuantityPerParent != null);

            if (productUnit == null)
                productUnit = product.UnitShares.FirstOrDefault();

            if (productUnit == null)
                return "0";

            var unit = productUnit.Unit;

            if ((UnitTypes)unit.Id == UnitTypes.Kilo)
                quantity /= 1000;

            // Calculate the total price based on unit price and quantity
            var str = $"{quantity}\t{unit.Name}";
            return str;
        }
        return "0"; // Return "0" if the value is not a ProductUnit
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
