using System.ComponentModel.DataAnnotations;

namespace POS_ModernUI.Models;

public class Supplier
{
    [Key]
    public int SupplierId { get; set; }
    public string Name { get; set; }

    [Required(ErrorMessage = "Phone number is required")]
    [RegularExpression(@"^01[0-9]{9}$", ErrorMessage = "Phone number must be 11 digits and start with 01")]
    public string ContactNumber { get; set; }
}