using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeddingApp.Data;
using WeddingApp.Models;

namespace WeddingApp.Controllers;

public class HomeController : Controller
{
    // DB context to access weddings for search functionality
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    // Public wedding search from home page
    [HttpGet]
    public async Task<IActionResult> Search(string query)
    {
        // Prevent empty saerch queries
        if (string.IsNullOrWhiteSpace(query))
        {
            return View("Index");
        }

        // Remove whitespace
        query = query.Trim();

        // Search for weddings based on couple name
        var weddings = await _context
            .Weddings.Where(w =>
                EF.Functions.Like(w.FirstPerson, $"%{query}%")
                || EF.Functions.Like(w.SecondPerson, $"%{query}%")
            )
            .ToListAsync();

        // Return results
        return View("SearchResults", weddings);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(
            new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }
        );
    }
}
