namespace WeddingApp.Models
{
    public class Wedding
    {
        // Properties
        public int WeddingId { get; set; } // pk
        public string UserId { get; set; } // fk
        public string FirstPerson { get; set; }
        public string SecondPerson { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public string Venue { get; set; }
        public string City { get; set; }
        public string? AdditionalInfo { get; set; }
        public string? HeaderImageUrl { get; set; }

        // Guests and gifts related to a wedding
        public ICollection<Guest> Guests { get; set; }
        public ICollection<Gift> Gifts { get; set; }
    }
}
