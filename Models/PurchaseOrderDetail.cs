using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POS_ModernUI.Models;

public class PurchaseOrderDetail
{
    [Key]
    public int PurchaseDetailId { get; set; }

    public int PurchaseOrderId { get; set; }
    public int ProductId { get; set; }
    [Range(0, double.MaxValue, ErrorMessage = "Unit cost must be a positive value.")]
    public decimal UnitCost { get; set; }
    [Range(0, double.MaxValue, ErrorMessage = "SubTotal must be a positive value.")]
    public decimal SubTotal { get; set; }
    [Range(0, int.MaxValue, ErrorMessage = "Quantity must be a non-negative number.")]
    public int Quantity { get; set; }

    [ForeignKey("PurchaseOrderId")]
    public PurchaseOrder PurchaseOrder { get; set; }

    [ForeignKey("ProductId")]
    public Product Product { get; set; }

}
