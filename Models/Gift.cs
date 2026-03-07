using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeddingApp.Models
{
    public class Gift
    {
        // Properties
        public int GiftId { get; set; } // pk
        public int WeddingId { get; set; } // fk

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Link is required")]
        [Url(ErrorMessage = "Please enter a valid URL")]
        [StringLength(200, ErrorMessage = "Link cannot exceed 200 characters")]
        public string? Link { get; set; }

        [Range(0.01, 1000000, ErrorMessage = "Price must be greater than 0")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:0.##}", ApplyFormatInEditMode = true)]
        public decimal? Price { get; set; }

        public bool IsReserved { get; set; }

        public string? GiftImagePath { get; set; }

        [NotMapped]
        public IFormFile? GiftImageFile { get; set; }

        // Navigation property
        public Wedding? Wedding { get; set; }
    }
}
