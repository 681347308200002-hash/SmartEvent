using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace SmartEvent.Models
{
    public class TicketPurchase
    {
        [Key]
        public int TicketPurchaseId { get; set; }

        [Required]
        public string UserId { get; set; } = "";

        [Required]
        public int EventId { get; set; }

        [Required]
        public int SeatTypeId { get; set; }

        [Required]
        [Range(1, 100)]
        public int Quantity { get; set; }

        [Required]
        [Range(0, 1000000)]
        public decimal UnitPrice { get; set; }

        [Required]
        [Range(0, 1000000)]
        public decimal TotalPrice { get; set; }

        public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;

        // Later we will store QR value here
        [StringLength(200)]
        public string? QrCodeValue { get; set; }

        // Navigation
        [ValidateNever]
        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(EventId))]
        public Event? Event { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(SeatTypeId))]
        public SeatType? SeatType { get; set; }
    }
}
