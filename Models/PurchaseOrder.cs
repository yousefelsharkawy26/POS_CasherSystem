using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POS_ModernUI.Models;

public class PurchaseOrder
{
    [Key]
    public int PurchaseOrderId { get; set; }
    public int SupplierId { get; set; }
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    public decimal TotalAmount { get; set; }

    [ForeignKey("SupplierId")]
    public Supplier Supplier { get; set; }
}
