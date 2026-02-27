namespace WeddingApp.Models
{
    public class Gift
    {
        // Properties
        public int GiftId { get; set; } // pk
        public int WeddingId { get; set; } // fk
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Link { get; set; }
        public decimal? Price { get; set; }
        public bool IsReserved { get; set; }
        public string? ImageUrl { get; set; }

        // Navigation property
        public Wedding Wedding { get; set; } = null!;
    }
}
