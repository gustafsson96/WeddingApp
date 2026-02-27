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

// Scaffolded and modified Wedding controller. Requires authorization.
namespace WeddingApp.Controllers
{
    public class WeddingsController : Controller
    {
        // Used to get the logged in user
        private readonly UserManager<IdentityUser> _userManager;

        // DbContext to access database
        private readonly ApplicationDbContext _context;

        public WeddingsController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager
        )
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Weddings
        public async Task<IActionResult> Index()
        {
            // Get logged in user
            var user = await _userManager.GetUserAsync(User);

            // Get wedding that belongs to logged in user
            var weddings = await _context.Weddings.Where(w => w.UserId == user.Id).ToListAsync();

            return View(weddings);
        }

        // GET: Weddings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Get wedding from database
            var wedding = await _context.Weddings.FirstOrDefaultAsync(m => m.WeddingId == id);

            if (wedding == null)
            {
                return NotFound();
            }

            // Get logged in user
            var user = await _userManager.GetUserAsync(User);

            // Safety check - only the owner can view their wedding
            if (wedding.UserId != user.Id)
            {
                return Forbid();
            }

            return View(wedding);
        }

        // GET: Weddings/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Weddings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("FirstPerson,SecondPerson,Date,Time,Venue,City,AdditionalInfo,HeaderImagePath")]
                Wedding wedding,
            IFormFile? headerImage
        )
        {
            if (ModelState.IsValid)
            {
                // Get logged in user
                var user = await _userManager.GetUserAsync(User);
                wedding.UserId = user.Id;

                // Generate unique slug
                wedding.PublicSlug = await GenerateUniqueSlug(
                    wedding.FirstPerson,
                    wedding.SecondPerson
                );

                // Handle uploaded header file
                if (headerImage != null && headerImage.Length > 0)
                {
                    // Buffer file in memory
                    using var memoryStream = new MemoryStream();
                    await headerImage.CopyToAsync(memoryStream);

                    // Create uploads folder if it does not exist
                    var uploadsFolder = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "uploads"
                    );
                    Directory.CreateDirectory(uploadsFolder);

                    // Give the header file a unique name
                    var uniqueFileName = $"{Guid.NewGuid()}_{headerImage.FileName}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Write buffered file to server
                    await System.IO.File.WriteAllBytesAsync(filePath, memoryStream.ToArray());

                    // Image path stored in database and used to display in views
                    wedding.HeaderImagePath = $"/uploads/{uniqueFileName}";
                }

                // Add to database
                _context.Add(wedding);
                await _context.SaveChangesAsync();

                // Redirect to dashboard after POST
                return RedirectToAction(nameof(Index));
            }
            return View(wedding);
        }

        // GET: Weddings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            // Get wedding
            var wedding = await _context.Weddings.FindAsync(id);

            if (wedding == null)
            {
                return NotFound();
            }

            // Get logged in user
            var user = await _userManager.GetUserAsync(User);

            // Safety check - only owner can edit
            if (wedding.UserId != user.Id)
            {
                return Forbid();
            }

            return View(wedding);
        }

        // POST: Weddings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind(
                "WeddingId,FirstPerson,SecondPerson,Date,Time,Venue,City,AdditionalInfo,HeaderImagePath"
            )]
                Wedding updatedWedding,
            IFormFile? headerImage
        )
        {
            // Check that id match for original and updated wedding
            if (id != updatedWedding.WeddingId)
            {
                return NotFound();
            }

            var wedding = await _context.Weddings.FindAsync(id);
            if (wedding == null)
            {
                return NotFound();
            }

            // Get logged in user
            var user = await _userManager.GetUserAsync(User);
            // Safety check - only owner can save edit
            if (wedding.UserId != user.Id)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                // Only update allowed fields
                wedding.FirstPerson = updatedWedding.FirstPerson;
                wedding.SecondPerson = updatedWedding.SecondPerson;
                wedding.Date = updatedWedding.Date;
                wedding.Time = updatedWedding.Time;
                wedding.Venue = updatedWedding.Venue;
                wedding.City = updatedWedding.City;
                wedding.AdditionalInfo = updatedWedding.AdditionalInfo;

                // Handle uploaded header image
                if (headerImage != null && headerImage.Length > 0)
                {
                    // Buffer file in memory
                    using var memoryStream = new MemoryStream();
                    await headerImage.CopyToAsync(memoryStream);

                    // Create uploads folder if it does not exist
                    var uploadsFolder = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "uploads"
                    );
                    Directory.CreateDirectory(uploadsFolder);

                    // Give the header file a unique name
                    var uniqueFileName = $"{Guid.NewGuid()}_{headerImage.FileName}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Write buffered file to server
                    await System.IO.File.WriteAllBytesAsync(filePath, memoryStream.ToArray());

                    // Update path for database and views
                    wedding.HeaderImagePath = $"/uploads/{uniqueFileName}";
                }

                // Redirect to dashboard after saving
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(updatedWedding);
        }

        // GET: Weddings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var wedding = await _context.Weddings.FirstOrDefaultAsync(m => m.WeddingId == id);
            if (wedding == null)
            {
                return NotFound();
            }

            // Get logged in user
            var user = await _userManager.GetUserAsync(User);

            // Safety check - only owner can delete
            if (wedding.UserId != user.Id)
            {
                return Forbid();
            }

            return View(wedding);
        }

        // POST: Weddings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var wedding = await _context.Weddings.FindAsync(id);
            if (wedding == null)
            {
                return NotFound();
            }
            // Get logged in user
            var user = await _userManager.GetUserAsync(User);

            // Safety check - only owner can perform a delete
            if (wedding.UserId != user.Id)
            {
                return Forbid();
            }

            _context.Weddings.Remove(wedding);
            await _context.SaveChangesAsync();

            // Redirect to dashboard after delete
            return RedirectToAction(nameof(Index));
        }

        // Create slug based on names of wedding couple
        private async Task<string> GenerateUniqueSlug(string first, string second)
        {
            // Replace Swedish characters
            string baseSlug = $"{first}-and-{second}"
                .ToLower()
                .Trim()
                .Replace("å", "a")
                .Replace("ä", "a")
                .Replace("ö", "o")
                .Replace(" ", "-");

            // Remove double hyphens
            while (baseSlug.Contains("--"))
            {
                baseSlug = baseSlug.Replace("--", "-");
            }

            string slug = baseSlug;
            int counter = 1;

            // Add number if the name combination already exists
            while (await _context.Weddings.AnyAsync(w => w.PublicSlug == slug))
            {
                slug = $"{baseSlug}-{counter}";
                counter++;
            }

            return slug;
        }

        // GET: Public wedding page based on PublicSlug
        [AllowAnonymous]
        [Route("{slug}")]
        public async Task<IActionResult> Public(string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return NotFound();
            }

            var wedding = await _context
                .Weddings.Include(w => w.Gifts)
                .FirstOrDefaultAsync(w => w.PublicSlug == slug);

            if (wedding == null)
            {
                return NotFound();
            }

            return View(wedding);
        }

        // GET: Wishlist page for a public wedding
        [AllowAnonymous]
        [Route("{slug}/wishlist")]
        public async Task<IActionResult> Wishlist(string slug)
        {
            // Return 404 is no slug is provided
            if (string.IsNullOrEmpty(slug))
                return NotFound();

            // Retrieve a wedding including gifts
            var wedding = await _context
                .Weddings.Include(w => w.Gifts)
                .FirstOrDefaultAsync(w => w.PublicSlug == slug);

            // Return 404 if no wedding is found
            if (wedding == null)
                return NotFound();

            return View(wedding);
        }

        // Handle a reservation of a gift on a public wedding site
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Reserve([FromBody] ReserveRequest request)
        {
            // Get the gift from the database
            var gift = await _context
                .Gifts.Include(g => g.Wedding)
                .FirstOrDefaultAsync(g => g.GiftId == request.GiftId && g.Wedding.PublicSlug == request.Slug);

            if (gift == null)
                return NotFound();

            // Check if a gift is reserved or not
            if (gift.IsReserved)
                return BadRequest("This gift is already reserved.");

            // Mark a gift as reserved
            gift.IsReserved = true;

            // Save to database
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
