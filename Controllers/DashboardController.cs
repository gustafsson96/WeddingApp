using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize] // Protected
public class DashboardController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
