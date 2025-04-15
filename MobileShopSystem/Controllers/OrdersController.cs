using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MobileShopSystem.ViewModels;
using Newtonsoft.Json;
using Stripe;
using Stripe.Checkout;

namespace MobileShopSystem.Controllers
{
    [Authorize(Roles = "User,Admin")]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrdersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        [Authorize(Roles = "User")]
        public async Task<IActionResult> UserOrders()
        {
            var userId = _userManager.GetUserId(User);
            var orders = await _context.Orders
                .Where(o => o.CustomerId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            ViewBag.OrdersCount = orders.Count;

            return View(orders);
        }

        [HttpGet]
        public IActionResult PayWithStripe(decimal amount, int orderId)
        {
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(amount * 100),
                            Currency = "egp",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"Order #{orderId}",
                            },
                        },
                        Quantity = 1,
                    },
                },
                Mode = "payment",
                SuccessUrl = Url.Action("PaymentSuccess", "Orders", new { orderId = orderId }, Request.Scheme),
                CancelUrl = Url.Action("PaymentCancel", "Orders", new { orderId = orderId }, Request.Scheme),
            };

            var service = new SessionService();
            Session session = service.Create(options);

            return Redirect(session.Url);
        }

        [HttpGet]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Checkout()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Checkout(OrderViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            var cart = await _context.ShoppingCarts
                .Include(c => c.CartItems)
                .ThenInclude(i => i.Mobile)
                .FirstOrDefaultAsync(c => c.CustomerId == user.Id);

            if (cart == null || !cart.CartItems.Any())
                return RedirectToAction("Index", "ShoppingCarts");

            var order = new Order
            {
                CustomerId = user.Id,
                OrderDate = DateTime.Now,
                Status = "Pending",
                PaymentMethod = model.PaymentMethod,
                PaymentStatus = "Not Paid",
                ShippingAddress = model.ShippingAddress,
                ShippingCity = model.ShippingCity,
                ShippingCountry = model.ShippingCountry,
                TotalAmount = cart.CartItems.Sum(i => i.UnitPrice * i.Quantity)
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            foreach (var item in cart.CartItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.OrderId,
                    MobileId = item.MobileId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                };
                _context.OrderItems.Add(orderItem);
            }

            _context.CartItems.RemoveRange(cart.CartItems);
            await _context.SaveChangesAsync();

            if (model.PaymentMethod == "Visa")
            {
                return RedirectToAction("PayWithStripe", new { amount = order.TotalAmount, orderId = order.OrderId });
            }
            else
            {
                return RedirectToAction("Confirmation", new { orderId = order.OrderId });
            }
        }

        public IActionResult PaymentSuccess(int orderId)
        {
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == orderId);

            if (order == null)
                return NotFound();

            order.PaymentStatus = "Paid";
            order.Status = "Confirmed";
            _context.SaveChanges();

            var viewModel = new OrderConfirmationViewModel
            {
                OrderId = order.OrderId,
                TotalAmount = order.TotalAmount
            };

            return View("Confirmation", viewModel);
        }

        public IActionResult PaymentCancel(int orderId)
        {
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == orderId);

            if (order != null)
            {
                order.PaymentStatus = "Failed";
                order.Status = "Cancelled";
                _context.SaveChanges();
            }

            return View("PaymentFailed");
        }

        [HttpGet]
        [Authorize(Roles = "User")]
        public IActionResult Confirmation(int orderId)
        {
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == orderId);

            if (order == null)
                return NotFound();

            var viewModel = new OrderConfirmationViewModel
            {
                OrderId = order.OrderId,
                TotalAmount = order.TotalAmount
            };

            return View(viewModel);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Orders.Include(o => o.Customer);
            return View(await applicationDbContext.ToListAsync());
        }

        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        public IActionResult Create()
        {
            ViewData["CustomerId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Create([Bind("OrderId,CustomerId,OrderDate,TotalAmount,Status,PaymentMethod,PaymentStatus,ShippingAddress,ShippingCity,ShippingCountry")] Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CustomerId"] = new SelectList(_context.Users, "Id", "Id", order.CustomerId);
            return View(order);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            ViewBag.CustomerId = new SelectList(
                _context.Users.Select(u => new {
                    Id = u.Id,
                    FullName = u.FirstName + " " + u.LastName
                }),
                             "Id",
                            "FullName",
                    order.CustomerId
                        );

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,CustomerId,OrderDate,TotalAmount,Status,PaymentMethod,PaymentStatus,ShippingAddress,ShippingCity,ShippingCountry")] Order order)
        {
            if (id != order.OrderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.OrderId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.CustomerId = new SelectList(
                _context.Users.Select(u => new {
                            Id = u.Id,
                        FullName = u.FirstName + " " + u.LastName
                                     }),
                             "Id",
                            "FullName",
                    order.CustomerId
                        );

            return View(order);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.OrderId == id);
        }
    }
}
