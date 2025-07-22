using System.ComponentModel.DataAnnotations;

namespace POS_ModernUI.Models;
public class SalesCasherModel: ObservableObject
{
    private int _quantity = 0;
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public int MaxQuantity { get; set; }
    [Range(0, int.MaxValue, ErrorMessage = "Quantity must be a non-negative number.")]
    public int Quantity
    {
        get => _quantity;
        set
        {
            if (UnitName == "جرام" && value > 0 && value / 1000 <= MaxQuantity)
            {
                // If the unit is "جرام", convert grams to kilograms
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
                UpdateTotalPrice();
            }
            else if (value > 0 && value <= MaxQuantity)
            {
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
                UpdateTotalPrice();
            }

        }
    }
    [Range(0, double.MaxValue, ErrorMessage = "Unit price must be a positive number.")]
    public decimal UnitPrice { get; set; }
    public string UnitName { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Total price must be a positive number.")]
    public decimal TotalPrice { get; set; }


    private void UpdateTotalPrice()
    {
        TotalPrice = UnitPrice * Quantity;
        OnPropertyChanged(nameof(TotalPrice));
    }
}
