using System;

namespace HaircutBookingSystem.ViewModels
{
    public class CalendarRowViewModel
    {
        public DateTime StartTime { get; set; }

        public string FullName { get; set; } = "";
        public string Service { get; set; } = "";
        public string Email { get; set; } = "";
        public string? Phone { get; set; }

        public bool IsUnavailable { get; set; }

        public int? AppointmentId { get; set; }
        public int? UnavailableId { get; set; }
    }
}
