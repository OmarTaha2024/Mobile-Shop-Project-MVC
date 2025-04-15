using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

public class CartCountFilter : IAsyncActionFilter
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public CartCountFilter(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var controller = context.Controller as Controller;

        if (controller != null)
        {
            if (controller.User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(controller.User);
                var cartCount = await _context.CartItems
                    .Include(c => c.ShoppingCart)
                    .ThenInclude(c => c.Customer)
                    .CountAsync(c => c.ShoppingCart.CustomerId == userId);

                controller.ViewBag.CartCount = cartCount;
            }
            else
            {
                controller.ViewBag.CartCount = 0;
            }
        }

        await next();
    }
}
