using System.ComponentModel.DataAnnotations;

namespace POS_ModernUI.Models;
public class Product
{
    [Key]
    public int ProductId { get; set; }
    public string Name { get; set; }
    [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive value.")]
    public decimal UnitPrice { get; set; }
    public string UnitName { get; set; } = "قطعة";
    public string Image { get; set; } = "\\Images\\DefaultProduct.png"; // Default image path
    [Range(0, int.MaxValue)]
    public int QuantityInStock { get; set; } = 0;
    public string? Barcode { get; set; }
}
