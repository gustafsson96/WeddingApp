using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WeddingApp.Data;
using WeddingApp.Models;

namespace WeddingApp.Controllers
{
    [Authorize]
    public class GiftsController : Controller
    {
        // Used to access currently logged in user
        private readonly UserManager<IdentityUser> _userManager;

        // Db context
        private readonly ApplicationDbContext _context;

        // Constructor
        public GiftsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Gifts for logged in users wedding
        public async Task<IActionResult> Index()
        {
            // Get the logged in user
            var user = await _userManager.GetUserAsync(User);

            // Get the wedding for the logged in user
            var wedding = await _context.Weddings.FirstOrDefaultAsync(w => w.UserId == user.Id);

            // Prevent access and show message if no wedding has been created
            if (wedding == null)
                return BadRequest("You must create a wedding first.");

            // Get the gifts for the specific wedding
            var gifts = await _context
                .Gifts.Where(g => g.WeddingId == wedding.WeddingId)
                .ToListAsync();

            return View(gifts);
        }

        // GET: Gifts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            // Get gift by id
            var gift = await _context.Gifts.FindAsync(id);
            if (gift == null)
                return NotFound();

            // Verify that the gift belongs to the logged in user's wedding
            var user = await _userManager.GetUserAsync(User);
            var wedding = await _context.Weddings.FirstOrDefaultAsync(w => w.UserId == user.Id);

            // Prevent access if the gift does not belong to the logged in user's wedding
            if (wedding == null || gift.WeddingId != wedding.WeddingId)
                return Forbid();

            return View(gift);
        }

        // GET: Gifts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Gifts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            // Bind allowed properties
            [Bind("Name,Description,Link,Price,ImageUrl")] Gift gift
        )
        {
            if (!ModelState.IsValid)
                return View(gift);

            // Get logged in user
            var user = await _userManager.GetUserAsync(User);

            // Get logged in user's wedding
            var wedding = await _context.Weddings.FirstOrDefaultAsync(w => w.UserId == user.Id);

            // Prevent access and show message if no wedding has been created
            if (wedding == null)
                return BadRequest("You must create a wedding first.");

            // Set the gift to belong to the logged in user's wedding
            gift.WeddingId = wedding.WeddingId;

            // Set reserved to fault by default
            gift.IsReserved = false;

            // Save gift to the database
            _context.Add(gift);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Gifts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var gift = await _context.Gifts.FindAsync(id);
            if (gift == null)
                return NotFound();

            // Verify that the gift belongs to the user before allowing editing
            var user = await _userManager.GetUserAsync(User);
            var wedding = await _context.Weddings.FirstOrDefaultAsync(w => w.UserId == user.Id);

            if (wedding == null || gift.WeddingId != wedding.WeddingId)
                return Forbid();

            return View(gift);
        }

        // POST: Gifts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            // Bind only allowed properties
            [Bind("GiftId,Name,Description,Link,Price,ImageUrl")] Gift updatedGift
        )
        {
            if (id != updatedGift.GiftId)
                return NotFound();

            var gift = await _context.Gifts.FindAsync(id);
            if (gift == null)
                return NotFound();

            // Verify that the gift belongs to the user before saving
            var user = await _userManager.GetUserAsync(User);
            var wedding = await _context.Weddings.FirstOrDefaultAsync(w => w.UserId == user.Id);

            if (wedding == null || gift.WeddingId != wedding.WeddingId)
                return Forbid();

            if (ModelState.IsValid)
            {
                // Update allowed fields
                gift.Name = updatedGift.Name;
                gift.Description = updatedGift.Description;
                gift.Link = updatedGift.Link;
                gift.Price = updatedGift.Price;
                gift.ImageUrl = updatedGift.ImageUrl;

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(updatedGift);
        }

        // GET: Gifts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var gift = await _context.Gifts.FindAsync(id);
            if (gift == null)
                return NotFound();

            // Verify that the gift belongs to the user before allowing delete
            var user = await _userManager.GetUserAsync(User);
            var wedding = await _context.Weddings.FirstOrDefaultAsync(w => w.UserId == user.Id);

            if (wedding == null || gift.WeddingId != wedding.WeddingId)
                return Forbid();

            return View(gift);
        }

        // POST: Gifts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var gift = await _context.Gifts.FindAsync(id);
            if (gift == null)
                return NotFound();

            // Verify that the gift belongs to the user before deleting
            var user = await _userManager.GetUserAsync(User);
            var wedding = await _context.Weddings.FirstOrDefaultAsync(w => w.UserId == user.Id);

            if (wedding == null || gift.WeddingId != wedding.WeddingId)
                return Forbid();

            _context.Gifts.Remove(gift);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
