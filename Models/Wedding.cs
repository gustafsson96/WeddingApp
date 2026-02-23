using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace WeddingApp.Models
{
    public class Wedding
    {
        // pk
        public int WeddingId { get; set; }

        // Connect wedding to as user via IdentityUser (fk)
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public IdentityUser? Owner { get; set; }

        // Properties with wedding information
        public string FirstPerson { get; set; } = string.Empty;
        public string SecondPerson { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Time { get; set; } = "";
        public string Venue { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? AdditionalInfo { get; set; }
        public string? HeaderImageUrl { get; set; }

        // Guests and gifts related to a wedding
        public ICollection<Guest> Guests { get; set; } = new List<Guest>();
        public ICollection<Gift> Gifts { get; set; } = new List<Gift>();

        // Set unique slug for public url
        public string PublicSlug { get; set; }
    }
}
