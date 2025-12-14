using System;
using System.Linq;
using System.Threading.Tasks;
using HaircutBookingSystem.Models;
using HaircutBookingSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HaircutBookingSystem.Controllers
{
    [Authorize(Roles = "Admin,Stylist")]
    public class CalendarController : Controller
    {
        private readonly ApplicationDbContext _db;

        // Adjust to your shop hours
        private static readonly TimeSpan BusinessOpen = new(9, 0, 0);
        private static readonly TimeSpan BusinessClose = new(17, 0, 0);

        public CalendarController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? stylistId)
        {
            var vm = await BuildCalendarVmAsync(stylistId);
            return View(vm);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Manage(int? stylistId)
        {
            var vm = await BuildCalendarVmAsync(stylistId);
            return View("Manage", vm);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAppointment(int id, int? stylistId)
        {
            var appt = await _db.Appointments.FindAsync(id);
            if (appt != null)
            {
                _db.Appointments.Remove(appt);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Manage), new { stylistId });
        }

        // ADMIN: Create an unavailability RANGE (one record)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUnavailability(
            int stylistId,
            DateTime startDate,
            TimeSpan? startTime,
            bool startAllDay,
            DateTime endDate,
            TimeSpan? endTime,
            bool endAllDay,
            int? selectedStylistId)
        {
            var sDate = startDate.Date;
            var eDate = endDate.Date;

            if (eDate < sDate)
                return RedirectToAction(nameof(Manage), new { stylistId = selectedStylistId });

            var start = sDate + (startAllDay ? BusinessOpen : (startTime ?? BusinessOpen));
            var end = eDate + (endAllDay ? BusinessClose : (endTime ?? BusinessClose));

            // Clamp to business hours for the day edges
            if (start.TimeOfDay < BusinessOpen) start = start.Date + BusinessOpen;
            if (start.TimeOfDay > BusinessClose) start = start.Date + BusinessClose;

            if (end.TimeOfDay < BusinessOpen) end = end.Date + BusinessOpen;
            if (end.TimeOfDay > BusinessClose) end = end.Date + BusinessClose;

            if (end <= start)
                return RedirectToAction(nameof(Manage), new { stylistId = selectedStylistId });

            // Optional: avoid duplicates (exact same range)
            var exists = await _db.StylistUnavailabilities
                .AsNoTracking()
                .AnyAsync(u => u.StylistId == stylistId && u.StartDateTime == start && u.EndDateTime == end);

            if (!exists)
            {
                _db.StylistUnavailabilities.Add(new StylistUnavailability
                {
                    StylistId = stylistId,
                    StartDateTime = start,
                    EndDateTime = end
                });
                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Manage), new { stylistId = selectedStylistId });
        }

        // ADMIN: Delete an unavailability RANGE
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUnavailability(int id, int? stylistId)
        {
            var u = await _db.StylistUnavailabilities.FindAsync(id);
            if (u != null)
            {
                _db.StylistUnavailabilities.Remove(u);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Manage), new { stylistId });
        }

        private async Task<CalendarAppointmentsViewModel> BuildCalendarVmAsync(int? stylistId)
        {
            // Dropdown options
            var stylists = await _db.Stylists
                .AsNoTracking()
                .Select(s => new { s.StylistId, StylistName = s.StylistName ?? "" })
                .OrderBy(s => s.StylistName)
                .ToListAsync();

            var vm = new CalendarAppointmentsViewModel
            {
                SelectedStylistId = stylistId,
                Stylists = new System.Collections.Generic.List<SelectListItem>
                {
                    new SelectListItem { Value = "", Text = "All Stylists", Selected = !stylistId.HasValue }
                }
            };

            vm.Stylists.AddRange(stylists.Select(s => new SelectListItem
            {
                Value = s.StylistId.ToString(),
                Text = s.StylistName,
                Selected = stylistId.HasValue && stylistId.Value == s.StylistId
            }));

            // Appointments ONLY (no unavailable rows)
            var apptQuery = _db.Appointments
                .AsNoTracking()
                .Include(a => a.Service)
                .Where(a => !stylistId.HasValue || stylistId.Value <= 0 || a.StylistId == stylistId.Value);

            vm.Rows = await apptQuery
                .OrderBy(a => a.StartTime)
                .Select(a => new CalendarRowViewModel
                {
                    StartTime = a.StartTime,
                    FullName = a.FullName,
                    Service = a.Service != null ? a.Service.Name : "",
                    Email = a.Email,
                    Phone = a.Phone,
                    IsUnavailable = false,
                    AppointmentId = a.Id,
                    UnavailableId = null
                })
                .ToListAsync();

            // Unavailability list (separate table)
            var unavailQuery = _db.StylistUnavailabilities
                .AsNoTracking()
                .Include(u => u.Stylist)
                .Where(u => !stylistId.HasValue || stylistId.Value <= 0 || u.StylistId == stylistId.Value);

            vm.Unavailabilities = await unavailQuery
                .OrderBy(u => u.StartDateTime)
                .Select(u => new UnavailabilityRowViewModel
                {
                    Id = u.Id,
                    StylistName = u.Stylist.StylistName ?? "",
                    StartDateTime = u.StartDateTime,
                    EndDateTime = u.EndDateTime
                })
                .ToListAsync();

            return vm;
        }
    }
}
