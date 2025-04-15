using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

public class ApplicationUser : IdentityUser
{
    [Required]
    [StringLength(50)]
    public string FirstName { get; set; }

    [Required]
    [StringLength(50)]
    public string LastName { get; set; }

    [StringLength(200)]
    public string Address { get; set; }

    [DataType(DataType.Date)]
    public DateTime CreatedAt { get; set; }
    public virtual List<Order> Orders { get; set; }
    public virtual List<ShoppingCart> ShoppingCarts { get; set; }


}
