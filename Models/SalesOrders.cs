using System.ComponentModel.DataAnnotations;

namespace POS_ModernUI.Models;
public class SalesOrder
{
    [Key]
    public int SalesOrderId { get; set; }
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    [Range(0, double.MaxValue, ErrorMessage = "Total amount must be a positive value.")]
    public decimal TotalAmount { get; set; }
    public string SalesOrderBarcode { get; set; }
}
