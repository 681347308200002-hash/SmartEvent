using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartEvent.Models
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }

        [Required]
        [StringLength(150)]
        public string EventName { get; set; }

        [Required]
        [StringLength(50)]
        public string Category { get; set; }   // Music, Theatre, Exhibition, etc.

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime EventDate { get; set; }

        [Required]
        [StringLength(150)]
        public string Location { get; set; }

        [Required]
        [Range(0, 100000)]
        public decimal BasePrice { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        // Navigation properties
        public ICollection<SeatType> SeatTypes { get; set; }
        public ICollection<Review> Reviews { get; set; }
    }
}
