using System.Diagnostics;
using DotNetEnv;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using WeddingApp.Data;
using WeddingApp.Models;

namespace WeddingApp.Controllers;

public class HomeController : Controller
{
    // DB context to access weddings for search functionality
    private readonly ApplicationDbContext _context;

    // Env variable for SendGrid
    private readonly string _sendGridApiKey;

    public HomeController(ApplicationDbContext context)
    {
        DotNetEnv.Env.Load();
        _context = context;
        _sendGridApiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY")!;
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
            return View();
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
        ViewBag.SearchQuery = query;
        return View(weddings);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(
            new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }
        );
    }

    // GET: Home/Contact
    [HttpGet]
    public IActionResult Contact()
    {
        return View(new ContactFormModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Contact(ContactFormModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await SendContactEmail(model.Name, model.Email, model.Message);

        ModelState.Clear();

        ViewBag.Message = "Thank you for your message, we'll get back to you soon!";

        return View(new ContactFormModel());
    }

    // POST: Home/Contact
    private async Task SendContactEmail(string name, string email, string messageText)
    {
        var senderEmail = Environment.GetEnvironmentVariable("SENDGRID_SENDER_EMAIL")!;
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Ever After", "jugu2402@student.miun.se"));
        message.ReplyTo.Add(new MailboxAddress(name, email));
        message.To.Add(MailboxAddress.Parse(senderEmail));
        message.Subject = "New message for Ever After";

        message.Body = new TextPart("html")
        {
            Text =
                $@"
            Name: {name}<br/>
            Email: {email}<br/><br/>
            Message:<br/>
            {messageText}
        ",
        };

        using var client = new SmtpClient();
        await client.ConnectAsync(
            "smtp.sendgrid.net",
            587,
            MailKit.Security.SecureSocketOptions.StartTls
        );
        await client.AuthenticateAsync("apikey", _sendGridApiKey);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
