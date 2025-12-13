using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace HaircutBookingSystem.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) 
        {
        }

        public DbSet<Service> Services => Set<Service>();

        public DbSet<Stylist> Stylists { get; set; }

        public DbSet<Appointment> Appointments { get; set; } = default!;

        public DbSet<UnavailableSlot> UnavailableSlots { get; set; } = default!;

        public DbSet<StylistUnavailability> StylistUnavailabilities { get; set; } = default!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<UnavailableSlot>(e =>
            {
                e.HasIndex(x => new { x.StylistId, x.StartTime }).IsUnique();

                e.HasOne(x => x.Stylist)
                 .WithMany()
                 .HasForeignKey(x => x.StylistId)
                 .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<Service>()
                .Property(s => s.Price)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<Service>()
                .HasIndex(s => s.Name)
                .IsUnique(); // Optional: prevent duplicate names
            modelBuilder.Entity<Appointment>()

    .HasIndex(a => new { a.StylistId, a.StartTime })
    .IsUnique();

        }
    }
}
