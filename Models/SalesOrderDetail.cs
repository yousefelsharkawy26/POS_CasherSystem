using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POS_ModernUI.Models;

public class SalesOrderDetail
{
    [Key]
    public int SalesDetailId { get; set; }
    public int SalesOrderId { get; set; }
    public int ProductId { get; set; }
    [Range(0, double.MaxValue, ErrorMessage = "Unit cost must be a positive value.")]
    public decimal UnitCost { get; set; }
    [Range(0, double.MaxValue, ErrorMessage = "Sub total must be a positive value.")]
    public decimal SubTotal { get; set; }
    [Range(0, int.MaxValue, ErrorMessage = "Quantity must be a non-negative integer.")]
    public int Quantity { get; set; }

    [ForeignKey("SalesOrderId")]
    public SalesOrder SalesOrder { get; set; }
    [ForeignKey("ProductId")]
    public Product Product { get; set; }
    
}
