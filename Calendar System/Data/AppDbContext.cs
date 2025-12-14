// Data/AppDbContext.cs
using CalendarDemo.Models;
using Microsoft.EntityFrameworkCore;

namespace CalendarDemo.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<CalendarViewModel> CalendarEvents => Set<CalendarViewModel>();
    }
}
