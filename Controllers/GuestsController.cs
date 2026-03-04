using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetEnv;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MimeKit;
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

        // Env variables for SendGrid
        private readonly string _sendGridApiKey;
        private readonly string _sendGridSender;

        // Constructor
        public GuestsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;

            Env.Load();

            // Read SendGrid values
            _sendGridApiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY")!;
            _sendGridSender = Environment.GetEnvironmentVariable("SENDGRID_SENDER_EMAIL")!;
        }

        // GET: Guests
        // Show all guests for logged in users wedding
        public async Task<IActionResult> Index(string? filter)
        {
            var user = await _userManager.GetUserAsync(User);
            var wedding = await _context.Weddings.FirstOrDefaultAsync(w => w.UserId == user.Id);

            if (wedding == null)
                return BadRequest("You must create a wedding first.");

            var guests = await _context
                .Guests.Where(g => g.WeddingId == wedding.WeddingId)
                .ToListAsync();

            // Filter functionality based on status of guest
            ViewBag.CurrentFilter = filter ?? "all";

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

        // Send invitation with RSVP-link to a guest via email
        private async Task SendRSVPEmail(Guest guest)
        {
            // Skip if guest has no email
            if (string.IsNullOrWhiteSpace(guest.Email))
                return;

            // Get the logged-in user's wedding
            var user = await _userManager.GetUserAsync(User);
            var wedding = await _context.Weddings.FirstOrDefaultAsync(w => w.UserId == user.Id);

            // Create a new email message
            var message = new MimeMessage();
            message.From.Add(
                new MailboxAddress(
                    $"{wedding.FirstPerson} & {wedding.SecondPerson} - Wedding",
                    "jugu2402@student.miun.se"
                )
            );
            message.To.Add(MailboxAddress.Parse(guest.Email));
            message.Subject = "Wedding Invitation";

            // Generate link to RSVP-form using unique guest RSVPToken
            var rsvpLink = Url.Action(
                "RSVPForm",
                "Guests",
                new { token = guest.RSVPToken },
                Request.Scheme
            );

            // Set the email body
            message.Body = new TextPart("html")
            {
                Text =
                    $@"
            Dear {guest.Name},<br/><br/>
            You're invited to our wedding!<br/>
            Please RSVP using the link below:<br/>
            <a href='{rsvpLink}'>RSVP-form</a><br/><br/>
            Thank you :-)",
            };

            // Connect to SMTP server via MailKit to authenticate and send message before disconnecting again
            using var client = new SmtpClient();
            await client.ConnectAsync(
                "smtp.sendgrid.net",
                587,
                MailKit.Security.SecureSocketOptions.StartTls
            );
            await client.AuthenticateAsync("apikey", _sendGridApiKey);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            // Save datetime for when invitation was sent
            guest.InvitationSentAt = DateTime.UtcNow;
            _context.Update(guest);
            await _context.SaveChangesAsync();
        }

        // POST: Send invitation to one guest
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendInvitation(int id)
        {
            // Get guest from database
            var guest = await _context.Guests.FindAsync(id);
            if (guest == null)
                return NotFound();

            // Send the invitation email with RSVP link
            await SendRSVPEmail(guest);

            // Notify admin that email was sent and redirect
            TempData["Message"] = $"RSVP sent to {guest.Name}";
            return RedirectToAction(nameof(Index));
        }

        // POST: Send invitation to all guests
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendInvitations()
        {
            // Get logged in user
            var user = await _userManager.GetUserAsync(User);

            // Get logged in user's wedding and display message if no wedding is found
            var wedding = await _context.Weddings.FirstOrDefaultAsync(w => w.UserId == user.Id);
            if (wedding == null)
                return BadRequest("You have to create a wedding first. ");

            // Get guests for a wedding and filter out those who have not been sent an invitiation
            var guests = await _context
                .Guests.Where(g => g.WeddingId == wedding.WeddingId && g.InvitationSentAt == null)
                .ToListAsync();

            foreach (var guest in guests)
            {
                await SendRSVPEmail(guest);
            }

            TempData["Message"] = "RSVP forms sent to all guests who hadn't received one yet.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Display RSVP form to a guest using their token
        [AllowAnonymous]
        public async Task<IActionResult> RSVPForm(Guid token)
        {
            // Find the guest in the database using the unique token from the url
            var guest = await _context
                .Guests.Include(g => g.Wedding)
                .FirstOrDefaultAsync(g => g.RSVPToken == token);

            // Return 404 if no guest matches the token
            if (guest == null)
                return NotFound();

            return View(guest);
        }

        // POST: Handle RSVP form submission
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> RSVPForm(
            int guestId,
            bool attending,
            string? foodPref,
            string? allergies,
            string? message
        )
        {
            // Retrieve the guest from the database using guestId
            var guest = await _context.Guests.FindAsync(guestId);

            // Return 404 if no guest can be found
            if (guest == null)
                return NotFound();

            // Update the guests RSVP-details
            guest.Attending = attending;
            guest.FoodPref = foodPref;
            guest.Allergies = allergies;
            guest.Message = message;
            guest.RSVPTime = DateTime.UtcNow;

            // Save changes to database
            _context.Update(guest);
            await _context.SaveChangesAsync();

            // Redirect to a Thank You page after form has been submitted
            return View("ThankYou", guest);
        }
    }
}
