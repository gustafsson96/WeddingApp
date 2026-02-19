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

// Scaffolded and modified Guest controller. Requires authorization.
namespace WeddingApp.Controllers
{
    [Authorize]
    public class GuestsController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        // Constructor
        public GuestsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Guests
        // Show all guests for logged in users wedding
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var wedding = await _context.Weddings.FirstOrDefaultAsync(w => w.UserId == user.Id);

            if (wedding == null)
                return BadRequest("You must create a wedding first.");

            var guests = await _context
                .Guests.Where(g => g.WeddingId == wedding.WeddingId)
                .ToListAsync();
            ViewBag.WeddingId = wedding.WeddingId;
            return View(guests);
        }

        // GET: Guests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Get all guests for the wedding
            var guest = await _context.Guests.FirstOrDefaultAsync(m => m.GuestId == id);
            if (guest == null)
            {
                return NotFound();
            }

            return View(guest);
        }

        // GET: Guests/Create
        public async Task<IActionResult> Create()
        {
            // Get logged in user
            var user = await _userManager.GetUserAsync(User);

            // Get logged in users wedding
            var wedding = await _context
                .Weddings.Where(w => w.UserId == user.Id)
                .FirstOrDefaultAsync();

            if (wedding == null)
            {
                return BadRequest("You must create a wedding before adding guests.");
            }

            // Create a guest for the specific wedding and generate unique token
            var guest = new Guest { WeddingId = wedding.WeddingId, RSVPToken = Guid.NewGuid() };

            // Send guest to the view
            return View(guest);
        }

        // POST: Guests/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind(
                "GuestId,Name,Email,Attending,RSVPTime,FoodPref,Allergies,Message,RSVPToken,InvitationSentAt"
            )]
                Guest guest
        )
        {
            // Validate form data
            if (!ModelState.IsValid)
            {
                return View(guest);
            }

            // For safety - get logged in user and wedding again
            var user = await _userManager.GetUserAsync(User);
            var wedding = await _context.Weddings.FirstOrDefaultAsync(w => w.UserId == user.Id);

            if (wedding == null)
                return BadRequest("You must create a wedding first.");

            // Link guest to a specific wedding
            guest.WeddingId = wedding.WeddingId;

            // Create RSVPToken if it is missing
            if (guest.RSVPToken == Guid.Empty)
                guest.RSVPToken = Guid.NewGuid();

            // Add guest to database
            _context.Add(guest);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Guests/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var guest = await _context.Guests.FindAsync(id);
            if (guest == null)
            {
                return NotFound();
            }
            return View(guest);
        }

        // POST: Guests/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind(
                "GuestId,WeddingId,Name,Email,Attending,RSVPTime,FoodPref,Allergies,Message,RSVPToken,InvitationSentAt"
            )]
                Guest guest
        )
        {
            // Ensure the correct guest is being updated
            if (id != guest.GuestId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update and save
                    _context.Update(guest);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GuestExists(guest.GuestId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { weddingId = guest.WeddingId });
            }
            return View(guest);
        }

        // GET: Guests/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var guest = await _context.Guests.FirstOrDefaultAsync(m => m.GuestId == id);
            if (guest == null)
            {
                return NotFound();
            }

            return View(guest);
        }

        // POST: Guests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Get guest
            var guest = await _context.Guests.FindAsync(id);
            if (guest == null)
                return NotFound();

            // Save WeddingId for redirect
            var weddingId = guest.WeddingId;

            // Delete and save
            _context.Guests.Remove(guest);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { weddingId });
        }

        private bool GuestExists(int id)
        {
            return _context.Guests.Any(e => e.GuestId == id);
        }
    }
}
