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
    [Authorize(Roles = "Admin")]

    public class MobilesController : Controller
    {
        private readonly ApplicationDbContext _context;
        protected readonly UserManager<ApplicationUser> _userManager;


        public MobilesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Mobiles
        [AllowAnonymous]
        public async Task<IActionResult> Index(string brand)
        {
            var brands = await _context.Mobiles
                            .Select(m => m.Brand)
                            .Distinct()
                            .ToListAsync();

            ViewBag.Brands = new SelectList(brands);
            ViewBag.latestMobiles = await _context.Mobiles
              .OrderByDescending(m => m.CreatedAt)
              .Take(6)
              .ToListAsync();

            var mobiles = string.IsNullOrEmpty(brand)
                ? await _context.Mobiles.ToListAsync()
                : await _context.Mobiles.Where(m => m.Brand == brand).ToListAsync();

            return View(mobiles);
        }


        // GET: Mobiles/Details/5
        [AllowAnonymous]

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mobile = await _context.Mobiles
                .FirstOrDefaultAsync(m => m.MobileId == id);
            if (mobile == null)
            {
                return NotFound();
            }

            return View(mobile);
        }

        // GET: Mobiles/Create
    
        public IActionResult Create()
        {
            return View();
        }

        // POST: Mobiles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Create([Bind("MobileId,Brand,Model,Description,Price,StockQuantity,IsAvailable,ImageUrl,CreatedAt")] Mobile mobile)
        {
            if (ModelState.IsValid)
            {
                _context.Add(mobile);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(mobile);
        }

        // GET: Mobiles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mobile = await _context.Mobiles.FindAsync(id);
            if (mobile == null)
            {
                return NotFound();
            }
            return View(mobile);
        }

        // POST: Mobiles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MobileId,Brand,Model,Description,Price,StockQuantity,IsAvailable,ImageUrl,CreatedAt")] Mobile mobile)
        {
            if (id != mobile.MobileId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(mobile);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MobileExists(mobile.MobileId))
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
            return View(mobile);
        }

        // GET: Mobiles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mobile = await _context.Mobiles
                .FirstOrDefaultAsync(m => m.MobileId == id);
            if (mobile == null)
            {
                return NotFound();
            }

            return View(mobile);
        }

        // POST: Mobiles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var mobile = await _context.Mobiles.FindAsync(id);
            if (mobile != null)
            {
                _context.Mobiles.Remove(mobile);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MobileExists(int id)
        {
            return _context.Mobiles.Any(e => e.MobileId == id);
        }
    }
}
