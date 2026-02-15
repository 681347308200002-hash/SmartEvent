using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartEvent.Models
{
    public class SeatType
    {
        [Key]
        public int SeatTypeId { get; set; }

        [Required]
        [StringLength(50)]
        public string TypeName { get; set; }   // VIP, Regular, Student

        [Required]
        [Range(0, 100000)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, 10000)]
        public int QuantityAvailable { get; set; }

        [Required]
        [Range(0, 100000)]
        public int AvailableSeats { get; set; }   // remaining seats


        // Foreign Key
        [ForeignKey("Event")]
        public int EventId { get; set; }

        // Navigation
        public Event? Event { get; set; }
    }
}
