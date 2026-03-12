using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
        public async Task<IActionResult> Index(string? filter)
        {
            // Get the logged in user
            var user = await _userManager.GetUserAsync(User);

            // Get the wedding for the logged in user
            var wedding = await _context.Weddings.FirstOrDefaultAsync(w => w.UserId == user.Id);

            // Prevent access and show message if no wedding has been created
            if (wedding == null)
                return BadRequest("You must create a wedding first.");

            filter ??= "all";
            ViewBag.CurrentFilter = filter;

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
            [Bind("Name,Description,Link,Price")] Gift gift,
            IFormFile? giftImage
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

            // Handle optional image upload
            if (giftImage != null && giftImage.Length > 0)
            {
                // Buffer file in memory
                using var ms = new MemoryStream();
                await giftImage.CopyToAsync(ms);

                // Create uploads folder if it does not exist
                var uploadsFolder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "uploads"
                );
                Directory.CreateDirectory(uploadsFolder);

                // Give the gift image file a unique name
                var uniqueFileName = $"{Guid.NewGuid()}_{giftImage.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Write buffered file to server
                await System.IO.File.WriteAllBytesAsync(filePath, ms.ToArray());

                // Update path for database and views
                gift.GiftImagePath = $"/uploads/{uniqueFileName}";
            }

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
            [Bind("GiftId,Name,Description,Link,Price,GiftImageFile")] Gift updatedGift
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

                // Handle optional image
                if (updatedGift.GiftImageFile != null && updatedGift.GiftImageFile.Length > 0)
                {
                    // Create path to uploads folder
                    var uploadsFolder = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "uploads"
                    );
                    Directory.CreateDirectory(uploadsFolder);

                    // Give the new file a unique name
                    var uniqueFileName = $"{Guid.NewGuid()}_{updatedGift.GiftImageFile.FileName}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Read to memory before saving
                    using var ms = new MemoryStream();
                    await updatedGift.GiftImageFile.CopyToAsync(ms);
                    await System.IO.File.WriteAllBytesAsync(filePath, ms.ToArray());

                    // Update path in database
                    gift.GiftImagePath = $"/uploads/{uniqueFileName}";
                }

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
