using System;
using System.Collections.Generic;
using System.Linq;
using HaircutBookingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

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

        // In-memory list of stylists (no database changes required).
        private static readonly List<Stylist> _stylists = new()
        {
            new Stylist { Id = 1, Name = "Alex" },
            new Stylist { Id = 2, Name = "Jordan" },
            new Stylist { Id = 3, Name = "Sam" }
        };

        // Simple in-memory store for booked appointments so we can detect unavailable slots.
        // This does NOT change the database; it only lives while the app is running.
        private sealed class BookedSlot
        {
            public DateOnly Date { get; }
            public TimeSpan Time { get; }
            public int StylistId { get; }

            public BookedSlot(DateOnly date, TimeSpan time, int stylistId)
            {
                Date = date;
                Time = time;
                StylistId = stylistId;
            }
        }

        private static readonly List<BookedSlot> _bookedSlots = new();

        public BookingController()
        {
        }

        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.ServiceList = new SelectList(_services, nameof(Service.Id), nameof(Service.Name));
            ViewBag.StylistList = new SelectList(_stylists, nameof(Stylist.Id), nameof(Stylist.Name));
            return View(new BookingRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(BookingRequest model)
        {
            ViewBag.ServiceList = new SelectList(_services, nameof(Service.Id), nameof(Service.Name));
            ViewBag.StylistList = new SelectList(_stylists, nameof(Stylist.Id), nameof(Stylist.Name));

            if (!ModelState.IsValid)
                return View(model);

            if (model.Time is null)
            {
                ModelState.AddModelError(nameof(model.Time), "Please choose a time.");
                return View(model);
            }

            // Enforce 30-minute increments server-side
            var totalMinutes = model.Time.Value.TotalMinutes;
            if (totalMinutes % 30 != 0)
            {
                ModelState.AddModelError(nameof(model.Time),
                    "Please choose a time in 30-minute increments (for example, 9:00, 9:30, 10:00).");
                return View(model);
            }

            if (!model.Date.HasValue)
            {
                ModelState.AddModelError(nameof(model.Date), "Please choose a date.");
                return View(model);
            }

            if (model.StylistId == 0)
            {
                ModelState.AddModelError(nameof(model.StylistId), "Please choose a stylist.");
                return View(model);
            }

            var chosenService = _services.First(s => s.Id == model.ServiceId);
            var chosenStylist = _stylists.First(s => s.Id == model.StylistId);

            var appointmentDate = DateOnly.FromDateTime(model.Date.Value.Date);
            var appointmentTime = model.Time.Value;
            var dateText = model.Date.Value.ToString("dddd, MMM d, yyyy");
            var timeText = appointmentTime.ToString(@"hh\:mm");

            // Check if this exact date/time/stylist combination is already booked.
            var isTaken = _bookedSlots.Any(b =>
                b.Date == appointmentDate &&
                b.Time == appointmentTime &&
                b.StylistId == model.StylistId);

            if (isTaken)
            {
                TempData["CustomerName"] = model.FullName;
                TempData["ServiceName"] = chosenService.Name;
                TempData["StylistName"] = chosenStylist.Name;
                TempData["DateText"] = dateText;
                TempData["TimeText"] = timeText;

                return RedirectToAction(nameof(Unavailable));
            }

            // Otherwise, mark this slot as booked (in-memory only; no database changes).
            _bookedSlots.Add(new BookedSlot(appointmentDate, appointmentTime, model.StylistId));

            // Pass booking details to the success page (screen-only confirmation, no email).
            TempData["CustomerName"] = model.FullName;
            TempData["ServiceName"] = chosenService.Name;
            TempData["StylistName"] = chosenStylist.Name;
            TempData["DateText"] = dateText;
            TempData["TimeText"] = timeText;

            return RedirectToAction(nameof(Success));
        }

        public IActionResult Success()
        {
            // Renders Views/Booking/Success.cshtml
            return View();
        }

        public IActionResult Unavailable()
        {
            // Renders Views/Booking/Unavailable.cshtml when a slot is already taken.
            return View();
        }
    }
}