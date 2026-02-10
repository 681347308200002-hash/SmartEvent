using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartEvent.Models
{
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [StringLength(500)]
        public string Comment { get; set; }

        public DateTime ReviewDate { get; set; } = DateTime.Now;

        // Foreign Key
        public int EventId { get; set; }

        // Navigation
        public Event Event { get; set; }
    }
}

