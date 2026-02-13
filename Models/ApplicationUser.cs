using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SmartEvent.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [StringLength(200)]
        public string EventPreferences { get; set; }
    }
}

