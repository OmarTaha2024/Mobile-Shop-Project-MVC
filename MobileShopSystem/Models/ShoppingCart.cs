using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

public class ShoppingCart
{
    [Key]
    public int CartId { get; set; }

    [ForeignKey("ApplicationUser")]
    public string CustomerId { get; set; }
    [ValidateNever]
    public virtual ApplicationUser Customer { get; set; }

    [DataType(DataType.Date)]
    public DateTime CreatedAt { get; set; }
    [ValidateNever]
    public virtual List<CartItem> CartItems { get; set; }
}
