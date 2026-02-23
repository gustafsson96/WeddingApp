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
            [Bind("FirstPerson,SecondPerson,Date,Time,Venue,City,AdditionalInfo,HeaderImageUrl")]
                Wedding wedding
        )
        {
            if (ModelState.IsValid)
            {
                // Get logged in user
                var user = await _userManager.GetUserAsync(User);

                // Link wedding to user
                wedding.UserId = user.Id;

                // Generate unique slug
                wedding.PublicSlug = await GenerateUniqueSlug(
                    wedding.FirstPerson,
                    wedding.SecondPerson
                );

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
                "WeddingId,FirstPerson,SecondPerson,Date,Time,Venue,City,AdditionalInfo,HeaderImageUrl"
            )]
                Wedding updatedWedding
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
                wedding.HeaderImageUrl = updatedWedding.HeaderImageUrl;

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
            string baseSlug = $"{first}-och-{second}"
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
    }
}
