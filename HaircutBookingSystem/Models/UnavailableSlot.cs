using System;
using System.ComponentModel.DataAnnotations;

namespace HaircutBookingSystem.Models
{
    public class UnavailableSlot
    {
        [Key]
        public int Id { get; set; }

        public int StylistId { get; set; }
        public Stylist Stylist { get; set; } = default!;

        public DateTime StartTime { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
