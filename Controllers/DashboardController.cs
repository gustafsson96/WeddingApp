using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeddingApp.Data;
using WeddingApp.Models;

[Authorize]
public class DashboardController : Controller
{
    // Used to access currently logged in user
    private readonly UserManager<IdentityUser> _userManager;

    // DB context
    private readonly ApplicationDbContext _context;

    // Constructor
    public DashboardController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        // Get  logged in user
        var user = await _userManager.GetUserAsync(User);

        // Get wedding that belongs to logged in user
        var wedding = await _context.Weddings.FirstOrDefaultAsync(w => w.UserId == user.Id);

        return View(wedding);
    }
}
