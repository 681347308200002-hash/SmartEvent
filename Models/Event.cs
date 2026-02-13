using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

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

        [StringLength(300)]
        public string? PosterPath { get; set; }

        // Navigation properties
        [ValidateNever]
        public ICollection<Review> Reviews { get; set; } = new List<Review>();

        [ValidateNever]
        public ICollection<SeatType> SeatTypes { get; set; } = new List<SeatType>();

    }
}
