namespace WeddingApp.Models
{
    // Bind JSON data from reserve AJAX request in WeddingsController
    public class ReserveRequest
    {
        public int GiftId { get; set; }
        public string? Slug { get; set; }
    }
}
