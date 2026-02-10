namespace WeddingApp.Models
{
    public class Guest
    {
        // Properties
        public int GuestId { get; set; } // pk
        public int WeddingId { get; set; } // fk
        public string Name { get; set; } = string.Empty;
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
