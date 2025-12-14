using System.Collections.Generic;
using HaircutBookingSystem.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HaircutBookingSystem.ViewModels
{
    public class CalendarAppointmentsViewModel
    {
        public int? SelectedStylistId { get; set; }
        public List<SelectListItem> Stylists { get; set; } = new();
        public List<Appointment> Appointments { get; set; } = new();
        public System.Collections.Generic.List<CalendarRowViewModel> Rows { get; set; } = new();
        public System.Collections.Generic.List<UnavailabilityRowViewModel> Unavailabilities { get; set; } = new();


    }
}
