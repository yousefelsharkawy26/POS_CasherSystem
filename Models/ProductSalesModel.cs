using LiveCharts;

namespace POS_ModernUI.Models
{
    public class ProductSalesModel
    {
        public string[] ProductNames { get; set; }
        public ChartValues<int> Quantities { get; set; }
    }
}
