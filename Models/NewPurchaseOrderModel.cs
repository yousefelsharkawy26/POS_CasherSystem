using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace POS_ModernUI.Models;
public class NewPurchaseOrderModel
{
    
    public int Id { get; set; }
    public string ProductCode { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public int MaxQuantity { get; set; }

    public decimal UnitCost { get; set; }
    public decimal SubTotal { get; set; }

    public ObservableCollection<string> Units { get; } = new()
    {
        "قطعة",
        "كيلو",
        "جرام",
    };

    public string SelectedUnit { get; set; } = "قطعة";

    public bool IsReadOnly { get; set; } = true;
}
