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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Service>()
                .Property(s => s.Price)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<Service>()
                .HasIndex(s => s.Name)
                .IsUnique(); // Optional: prevent duplicate names
        }
    }
}
