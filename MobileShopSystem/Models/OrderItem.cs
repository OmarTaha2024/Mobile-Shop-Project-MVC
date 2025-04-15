using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class OrderItem
{
    public int OrderItemId { get; set; }

    [ForeignKey("Order")]
    public int OrderId { get; set; }

    public virtual Order Order { get; set; }

    [ForeignKey("Mobile")]
    public int MobileId { get; set; }

    public virtual Mobile Mobile { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
    public int Quantity { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Unit Price must be greater than 0.")]
    public decimal UnitPrice { get; set; }

    public decimal TotalPrice => UnitPrice * Quantity;
}
