using System.ComponentModel.DataAnnotations;

namespace SmartEvent.ViewModels
{
    public class InquiryCreateVM
    {
        [StringLength(100)]
        public string? Name { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; } = "";

        [Required]
        [StringLength(150)]
        public string Subject { get; set; } = "";

        [Required]
        [StringLength(2000)]
        public string Message { get; set; } = "";

        // Optional: if you want to link inquiry to a specific event
        public int? EventId { get; set; }
    }
}
