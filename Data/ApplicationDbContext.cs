using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WeddingApp.Models;

namespace WeddingApp.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // Custom tables for weddings, guests and gifts
        public DbSet<Wedding> Weddings { get; set; }
        public DbSet<Guest> Guests { get; set; }
        public DbSet<Gift> Gifts { get; set; }
    }
}
