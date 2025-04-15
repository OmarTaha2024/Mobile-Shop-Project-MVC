using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MobileShopSystem.Controllers
{
    [Authorize(Roles = "User")]
    public class ShoppingCartsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ShoppingCartsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> AddToCart(int mobileId)
        {
            var user = await _userManager.GetUserAsync(User);

            var cart = _context.ShoppingCarts
                .Include(c => c.CartItems)
                .FirstOrDefault(c => c.CustomerId == user.Id);

            if (cart == null)
            {
                cart = new ShoppingCart
                {
                    CustomerId = user.Id,
                    CreatedAt = DateTime.Now
                };
                _context.ShoppingCarts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var existingItem = cart.CartItems.FirstOrDefault(i => i.MobileId == mobileId);
            if (existingItem != null)
            {
                existingItem.Quantity += 1;
            }
            else
            {
                var mobile = await _context.Mobiles.FindAsync(mobileId);
                if (mobile != null)
                {
                    cart.CartItems.Add(new CartItem
                    {
                        MobileId = mobile.MobileId,
                        Quantity = 1,
                        UnitPrice = mobile.Price,
                        AddedAt = DateTime.Now
                    });
                }
            }

            await _context.SaveChangesAsync();


            return Redirect(Request.Headers["Referer"].ToString());
        }

        // GET: ShoppingCarts
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var cart = _context.ShoppingCarts
                .Include(c => c.CartItems)
                .ThenInclude(i => i.Mobile)
                .FirstOrDefault(c => c.CustomerId == user.Id);

            if (cart == null)
            {
                cart = new ShoppingCart
                {
                    CustomerId = user.Id,
                    CreatedAt = DateTime.Now,
                    CartItems = new List<CartItem>()
                };
            }

            return View(cart); 

        }

        // GET: ShoppingCarts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shoppingCart = await _context.ShoppingCarts
                .Include(s => s.Customer)
                .FirstOrDefaultAsync(m => m.CartId == id);
            if (shoppingCart == null)
            {
                return NotFound();
            }

            return View(shoppingCart);
        }

        // POST: ShoppingCarts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CartId,CustomerId,CreatedAt")] ShoppingCart shoppingCart)
        {
            if (id != shoppingCart.CartId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(shoppingCart);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ShoppingCartExists(shoppingCart.CartId))
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
            ViewData["CustomerId"] = new SelectList(_context.Users, "Id", "Id", shoppingCart.CustomerId);
            return View(shoppingCart);
        }

        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> RemoveItem(int? id)
        {
            var cartItem = await _context.CartItems.FindAsync(id);

            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }
            return Redirect(Request.Headers["Referer"].ToString());
        }
        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> UpdateQuantity(int? cartItemId, string act)
        {
            var cartItem = await _context.CartItems.FindAsync(cartItemId);
            if (cartItem == null)
                return NotFound();

            if (act == "increase")
                cartItem.Quantity++;
            else if (act == "decrease" && cartItem.Quantity > 1)
                cartItem.Quantity--;

            await _context.SaveChangesAsync();
            return Redirect(Request.Headers["Referer"].ToString());
        }

        private bool ShoppingCartExists(int id)
        {
            return _context.ShoppingCarts.Any(e => e.CartId == id);
        }
    }
}
