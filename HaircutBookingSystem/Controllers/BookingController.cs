using System;
using System.Collections.Generic;
using System.Linq;
using HaircutBookingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace HaircutBookingSystem.Controllers
{
    public class BookingController : Controller
    {
        // In-memory list of services (still not using the database for this demo).
        private static readonly List<Service> _services = new()
        {
            new Service { Id = 1, Name = "Haircut",       Price = 25 },
            new Service { Id = 2, Name = "Hair Styling",  Price = 40 },
            new Service { Id = 3, Name = "Hair Coloring", Price = 80 },
            new Service { Id = 4, Name = "Beard Trim",    Price = 15 }
        };

        // In-memory list of stylists.
        private static readonly List<Stylist> _stylists = new()
        {
            new Stylist { StylistId = 1, StylistName = "Alex" },
            new Stylist { StylistId = 2, StylistName = "Jordan" },
            new Stylist { StylistId = 3, StylistName = "Sam" }
        };

        // Simple in-memory store for booked appointments.
        private sealed class BookedSlot
        {
            public DateOnly Date { get; }
            public TimeSpan Time { get; }
            public int StylistId { get; }

            public string CustomerName { get; }
            public string Email { get; }

            // Has a reminder already been sent for this appointment?
            public bool ReminderSent { get; set; }

            // Combined Date + Time as a DateTime for comparisons
            public DateTime AppointmentDateTime =>
                new DateTime(Date.Year, Date.Month, Date.Day,
                             Time.Hours, Time.Minutes, Time.Seconds);

            public BookedSlot(DateOnly date, TimeSpan time, int stylistId, string customerName, string email)
            {
                Date = date;
                Time = time;
                StylistId = stylistId;
                CustomerName = customerName;
                Email = email;
                ReminderSent = false;
            }
        }

        private static readonly List<BookedSlot> _bookedSlots = new();

        // Logger + reminder configuration
        private readonly ILogger<BookingController> _logger;
        private const int ReminderHoursBefore = 24; // send reminders 24 hours before appointment

        public BookingController(ILogger<BookingController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.ServiceList = new SelectList(_services, nameof(Service.Id), nameof(Service.Name));
            ViewBag.StylistList = new SelectList(_stylists, nameof(Stylist.StylistId), nameof(Stylist.StylistName));

            return View(new BookingRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(BookingRequest model)
        {
            // Repopulate dropdowns when returning the view
            ViewBag.ServiceList = new SelectList(_services, nameof(Service.Id), nameof(Service.Name));
            ViewBag.StylistList = new SelectList(_stylists, nameof(Stylist.StylistId), nameof(Stylist.StylistName));

            // 1. Run all model-level validation first
            //    - Required, Range, email, phone
            //    - IValidatableObject (future date/time, opening hours)
            if (!ModelState.IsValid)
                return View(model);

            // 2. Ensure the selected service actually exists
            var chosenService = _services.FirstOrDefault(s => s.Id == model.ServiceId);
            if (chosenService == null)
            {
                ModelState.AddModelError(nameof(model.ServiceId), "The selected service is invalid.");
                return View(model);
            }

            // 3. Ensure the selected stylist actually exists
            var chosenStylist = _stylists.FirstOrDefault(s => s.StylistId == model.StylistId);
            if (chosenStylist == null)
            {
                ModelState.AddModelError(nameof(model.StylistId), "The selected stylist is invalid.");
                return View(model);
            }

            // 4. Enforce 30-minute increments server-side
            //    (we know Time has a value because ModelState is valid)
            var time = model.Time!.Value;
            var totalMinutes = time.TotalMinutes;
            if (totalMinutes % 30 != 0)
            {
                ModelState.AddModelError(nameof(model.Time),
                    "Please choose a time in 30-minute increments (for example, 9:00, 9:30, 10:00).");
                return View(model);
            }

            // 5. Build appointment date/time
            var appointmentDate = DateOnly.FromDateTime(model.Date!.Value.Date);
            var appointmentTime = time;
            var dateText = model.Date.Value.ToString("dddd, MMM d, yyyy");
            var timeText = appointmentTime.ToString(@"hh\:mm");

            // 6. Check if this exact date/time/stylist is already booked
            var isTaken = _bookedSlots.Any(b =>
                b.Date == appointmentDate &&
                b.Time == appointmentTime &&
                b.StylistId == model.StylistId);

            if (isTaken)
            {
                TempData["CustomerName"] = model.FullName;
                TempData["ServiceName"] = chosenService.Name;
                TempData["StylistName"] = chosenStylist.StylistName;
                TempData["DateText"] = dateText;
                TempData["TimeText"] = timeText;

                // Backend rejects the booking as unavailable
                return RedirectToAction(nameof(Unavailable));
            }

            // 7. Slot is free: mark it booked (in-memory only) INCLUDING email + name for reminders
            _bookedSlots.Add(new BookedSlot(
                appointmentDate,
                appointmentTime,
                model.StylistId,
                model.FullName,
                model.Email));

            // Pass booking details to the success page (screen-only confirmation)
            TempData["CustomerName"] = model.FullName;
            TempData["ServiceName"] = chosenService.Name;
            TempData["StylistName"] = chosenStylist.StylistName;
            TempData["DateText"] = dateText;
            TempData["TimeText"] = timeText;

            return RedirectToAction(nameof(Success));
        }

        public IActionResult Success()
        {
            return View();
        }

        public IActionResult Unavailable()
        {
            return View();
        }

        // -------- Sprint 1: email reminders --------

        // Manually trigger sending reminders (e.g., via URL /Booking/SendReminders)
        public IActionResult SendReminders()
        {
            var now = DateTime.Now;

            // Find appointments in the next 24 hours that haven't been reminded yet
            var remindersToSend = _bookedSlots
                .Where(b => !b.ReminderSent)
                .Where(b => b.AppointmentDateTime > now)
                .Where(b => (b.AppointmentDateTime - now).TotalHours <= ReminderHoursBefore)
                .ToList();

            foreach (var slot in remindersToSend)
            {
                try
                {
                    // Simulate sending an email reminder by logging it
                    _logger.LogInformation(
                        "Sending reminder email to {Email} for appointment on {DateTime} (Customer: {Customer})",
                        slot.Email,
                        slot.AppointmentDateTime,
                        slot.CustomerName);

                    // TODO (real system): call email service / SMTP client here.

                    slot.ReminderSent = true;
                }
                catch (Exception ex)
                {
                    // Log any errors while "sending" reminders
                    _logger.LogError(
                        ex,
                        "Error sending reminder to {Email} for appointment on {DateTime}",
                        slot.Email,
                        slot.AppointmentDateTime);
                }
            }

            TempData["RemindersSentCount"] = remindersToSend.Count;
            return RedirectToAction(nameof(RemindersResult));
        }

        public IActionResult RemindersResult()
        {
            ViewBag.RemindersSentCount = TempData["RemindersSentCount"] ?? 0;
            return View();
        }
    }
}
