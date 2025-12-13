using System;
using System.Linq;
using System.Threading.Tasks;
using HaircutBookingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HaircutBookingSystem.Controllers
{
    public class BookingController : Controller
    {
        private readonly ILogger<BookingController> _logger;
        private readonly ApplicationDbContext _db;

        public BookingController(ILogger<BookingController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await PopulateDropdownsAsync();
            return View(new BookingRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(BookingRequest model)
        {
            await PopulateDropdownsAsync();

            if (!ModelState.IsValid)
                return View(model);

            // Service exists?
            var chosenService = await _db.Services
                .AsNoTracking()
                .Select(s => new { s.Id, s.Name })
                .FirstOrDefaultAsync(s => s.Id == model.ServiceId);

            if (chosenService == null)
            {
                ModelState.AddModelError(nameof(model.ServiceId), "The selected service is invalid.");
                return View(model);
            }

            // Stylist exists?
            var chosenStylist = await _db.Stylists
                .AsNoTracking()
                .Select(s => new { s.StylistId, StylistName = s.StylistName ?? "" })
                .FirstOrDefaultAsync(s => s.StylistId == model.StylistId);

            if (chosenStylist == null)
            {
                ModelState.AddModelError(nameof(model.StylistId), "The selected stylist is invalid.");
                return View(model);
            }

            // 30-min increments
            var time = model.Time!.Value;
            if (time.TotalMinutes % 30 != 0)
            {
                ModelState.AddModelError(nameof(model.Time),
                    "Please choose a time in 30-minute increments (for example, 9:00, 9:30, 10:00).");
                return View(model);
            }

            var start = model.Date!.Value.Date + time;

            var dateText = model.Date.Value.ToString("dddd, MMM d, yyyy");
            var timeText = start.ToString("hh:mm tt");

            // Appointment already taken?
            var isTaken = await _db.Appointments
                .AsNoTracking()
                .AnyAsync(a => a.StylistId == model.StylistId && a.StartTime == start);

            // Blocked by unavailability range?
            var isBlocked = await _db.StylistUnavailabilities
                .AsNoTracking()
                .AnyAsync(u =>
                    u.StylistId == model.StylistId &&
                    start >= u.StartDateTime &&
                    start < u.EndDateTime);

            if (isTaken || isBlocked)
            {
                SetTempData(model.FullName, chosenService.Name, chosenStylist.StylistName, dateText, timeText);
                return RedirectToAction(nameof(Unavailable));
            }

            var appt = new Appointment
            {
                StartTime = start,
                FullName = model.FullName,
                Email = model.Email,
                Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone,
                ServiceId = chosenService.Id,
                StylistId = chosenStylist.StylistId
            };

            _db.Appointments.Add(appt);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                SetTempData(model.FullName, chosenService.Name, chosenStylist.StylistName, dateText, timeText);
                return RedirectToAction(nameof(Unavailable));
            }

            _logger.LogInformation("Booked: {Start} {Name} {Service} StylistId={StylistId}",
                start, model.FullName, chosenService.Name, chosenStylist.StylistId);

            SetTempData(model.FullName, chosenService.Name, chosenStylist.StylistName, dateText, timeText);
            return RedirectToAction(nameof(Success));
        }

        public IActionResult Success() => View();
        public IActionResult Unavailable() => View();

        private async Task PopulateDropdownsAsync()
        {
            var services = await _db.Services
                .AsNoTracking()
                .Select(s => new { s.Id, s.Name })
                .OrderBy(s => s.Name)
                .ToListAsync();

            var stylists = await _db.Stylists
                .AsNoTracking()
                .Select(s => new { s.StylistId, StylistName = s.StylistName ?? "" })
                .OrderBy(s => s.StylistName)
                .ToListAsync();

            ViewBag.ServiceList = new SelectList(services, "Id", "Name");
            ViewBag.StylistList = new SelectList(stylists, "StylistId", "StylistName");
        }

        private void SetTempData(string customerName, string serviceName, string stylistName, string dateText, string timeText)
        {
            TempData["CustomerName"] = customerName;
            TempData["ServiceName"] = serviceName;
            TempData["StylistName"] = stylistName;
            TempData["DateText"] = dateText;
            TempData["TimeText"] = timeText;
        }
    }
}
