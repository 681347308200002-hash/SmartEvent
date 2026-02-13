using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartEvent.Models;

namespace SmartEvent.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Event> Events { get; set; }
        public DbSet<SeatType> SeatTypes { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Event>()
                .Property(e => e.BasePrice)
                .HasPrecision(10, 2);

            builder.Entity<SeatType>()
                .Property(s => s.Price)
                .HasPrecision(10, 2);
        }

    }

}
