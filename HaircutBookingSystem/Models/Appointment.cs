using System;
using System.ComponentModel.DataAnnotations;

namespace HaircutBookingSystem.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }

        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? Phone { get; set; }

        public int ServiceId { get; set; }
        public Service Service { get; set; } = default!;

        public int StylistId { get; set; }
        public Stylist Stylist { get; set; } = default!;
    }
}
