using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WeddingApp.Data;
using WeddingApp.Models;

namespace WeddingApp.Controllers
{
    public class WeddingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WeddingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Weddings
        public async Task<IActionResult> Index()
        {
            return View(await _context.Weddings.ToListAsync());
        }

        // GET: Weddings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var wedding = await _context.Weddings
                .FirstOrDefaultAsync(m => m.WeddingId == id);
            if (wedding == null)
            {
                return NotFound();
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
        public async Task<IActionResult> Create([Bind("WeddingId,UserId,FirstPerson,SecondPerson,Date,Time,Venue,City,AdditionalInfo,HeaderImageUrl")] Wedding wedding)
        {
            if (ModelState.IsValid)
            {
                _context.Add(wedding);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(wedding);
        }

        // GET: Weddings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var wedding = await _context.Weddings.FindAsync(id);
            if (wedding == null)
            {
                return NotFound();
            }
            return View(wedding);
        }

        // POST: Weddings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("WeddingId,UserId,FirstPerson,SecondPerson,Date,Time,Venue,City,AdditionalInfo,HeaderImageUrl")] Wedding wedding)
        {
            if (id != wedding.WeddingId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(wedding);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WeddingExists(wedding.WeddingId))
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
            return View(wedding);
        }

        // GET: Weddings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var wedding = await _context.Weddings
                .FirstOrDefaultAsync(m => m.WeddingId == id);
            if (wedding == null)
            {
                return NotFound();
            }

            return View(wedding);
        }

        // POST: Weddings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var wedding = await _context.Weddings.FindAsync(id);
            if (wedding != null)
            {
                _context.Weddings.Remove(wedding);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool WeddingExists(int id)
        {
            return _context.Weddings.Any(e => e.WeddingId == id);
        }
    }
}
