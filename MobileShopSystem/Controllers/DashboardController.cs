using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileShopSystem.ViewModels;
[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var totalSales = _context.Orders.Sum(o => o.TotalAmount);
        var totalOrders = _context.Orders.Count();
        var totalProducts = _context.Mobiles.Count();

        var pendingOrders = _context.Orders.Count(o => o.Status == "Pending");

        var dashboardData = new DashboardViewModel
        {
            TotalSales = totalSales,
            TotalOrders = totalOrders,
            TotalProducts = totalProducts,
            PendingOrders = pendingOrders
        };

        return View(dashboardData);
    }
}
