using System.ComponentModel.DataAnnotations;

namespace WeddingApp.Models
{
    public class ContactFormModel
    {
        [Required]
        [Display(Name = "name")]
        public string? Name { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "email")]
        public string? Email { get; set; }

        [Required]
        [Display(Name = "message")]
        public string? Message { get; set; }
    }
}
