using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

public class Mobile
{
    public int MobileId { get; set; }

    [Required]
    [StringLength(50, ErrorMessage = "Brand can't be longer than 50 characters.")]
    public string Brand { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = "Model can't be longer than 100 characters.")]
    public string Model { get; set; }

    [Required]
    [StringLength(500, ErrorMessage = "Description can't be longer than 500 characters.")]
    public string Description { get; set; }

    [Range(1000.00, double.MaxValue, ErrorMessage = "Price must be greater than 1000.")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Stock quantity must be a positive number.")]
    public int StockQuantity { get; set; }

    public bool IsAvailable { get; set; }

    [StringLength(500, ErrorMessage = "Image URL can't be longer than 500 characters.")]
    public string ImageUrl { get; set; }

    [DataType(DataType.Date)]
    public DateTime CreatedAt { get; set; }
    [ValidateNever]
    public virtual List<OrderItem> OrderItems { get; set; }
    [ValidateNever]
    public virtual List<CartItem> CartItems { get; set; }

}
