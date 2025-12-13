using System;
using System.ComponentModel.DataAnnotations;

namespace HaircutBookingSystem.Models
{
    public class StylistUnavailability
    {
        [Key]
        public int Id { get; set; }

        public int StylistId { get; set; }
        public Stylist Stylist { get; set; } = default!;

        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
    }
}
