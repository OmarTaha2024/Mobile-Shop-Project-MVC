using System.ComponentModel.DataAnnotations;

public class OrderViewModel
{
    [Required]
    public string PaymentMethod { get; set; }

    [Required]
    [StringLength(200)]
    public string ShippingAddress { get; set; }

    [Required]
    public string ShippingCity { get; set; }

    [Required]
    public string ShippingCountry { get; set; }
}
