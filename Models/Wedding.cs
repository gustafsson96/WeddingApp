namespace WeddingApp.Models
{
    public class Wedding
    {
        // Properties
        public int WeddingId { get; set; } // pk
        public string UserId { get; set; } = string.Empty; // fk
        public string FirstPerson { get; set; } = string.Empty;
        public string SecondPerson { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public string Venue { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? AdditionalInfo { get; set; }
        public string? HeaderImageUrl { get; set; }

        // Guests and gifts related to a wedding
        public ICollection<Guest> Guests { get; set; } = new List<Guest>();
        public ICollection<Gift> Gifts { get; set; } = new List<Gift>();
    }
}
