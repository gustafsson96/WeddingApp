using System.ComponentModel.DataAnnotations;

namespace WeddingApp.Models
{
    public class Guest
    {
        // Properties
        public int GuestId { get; set; } // pk
        public int WeddingId { get; set; } // fk

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [StringLength(150, ErrorMessage = "Email cannot exceed 150 characters")]
        public string Email { get; set; } = string.Empty;

        public bool? Attending { get; set; }
        public DateTime? RSVPTime { get; set; }
        public string? FoodPref { get; set; }
        public string? Allergies { get; set; }
        public string? Message { get; set; }
        public Guid RSVPToken { get; set; }
        public DateTime? InvitationSentAt { get; set; }
    }
}
