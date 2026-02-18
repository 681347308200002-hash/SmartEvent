using System;
using System.ComponentModel.DataAnnotations;

namespace SmartEvent.Models
{
    public enum InquiryStatus
    {
        Pending = 0,
        Replied = 1
    }

    public class Inquiry
    {
        public int InquiryId { get; set; }

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

        // Optional: link to an Event (guest can choose an event)
        public int? EventId { get; set; }
        public Event? Event { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public InquiryStatus Status { get; set; } = InquiryStatus.Pending;

        // Optional: internal note for admin (not required)
        [StringLength(500)]
        public string? AdminNotes { get; set; }
    }
}
