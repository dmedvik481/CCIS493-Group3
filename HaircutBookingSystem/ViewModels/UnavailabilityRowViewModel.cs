using System;

namespace HaircutBookingSystem.ViewModels
{
    public class UnavailabilityRowViewModel
    {
        public int Id { get; set; }
        public string StylistName { get; set; } = "";
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
    }
}
