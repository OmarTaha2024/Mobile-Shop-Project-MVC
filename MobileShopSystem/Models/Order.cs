using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

public class Order
{
    public int OrderId { get; set; }

    [ForeignKey("ApplicationUser")]
    public string CustomerId { get; set; }
    [ValidateNever]
    public virtual ApplicationUser Customer { get; set; }

    [DataType(DataType.Date)]
    public DateTime OrderDate { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Total Amount must be greater than 0.")]
    public decimal TotalAmount { get; set; }

    [Required]
    [StringLength(50, ErrorMessage = "Status can't be longer than 50 characters.")]
    public string Status { get; set; }

    [Required]
    [StringLength(50, ErrorMessage = "Payment Method can't be longer than 50 characters.")]
    public string PaymentMethod { get; set; }

    [Required]
    [StringLength(50, ErrorMessage = "Payment Status can't be longer than 50 characters.")]
    public string PaymentStatus { get; set; }

    [Required]
    [StringLength(200, ErrorMessage = "Shipping Address can't be longer than 200 characters.")]
    public string ShippingAddress { get; set; }

    [Required]
    [StringLength(50, ErrorMessage = "Shipping City can't be longer than 50 characters.")]
    public string ShippingCity { get; set; }

    [Required]
    [StringLength(50, ErrorMessage = "Shipping Country can't be longer than 50 characters.")]
    public string ShippingCountry { get; set; }
    [ValidateNever]
    public virtual List<OrderItem> OrderItems { get; set; }
}
