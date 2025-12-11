using System;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using CalendarDemo.Models;
using CalendarDemo.Services;

namespace CalendarDemo.Controllers
{
    public class CalendarController : Controller
    {
        private readonly InMemoryNoteStore _store;

        public CalendarController(InMemoryNoteStore store)
        {
            _store = store;
        }

        [HttpGet]
        public IActionResult Index(int? year, int? month)
        {
            var now = DateTime.Now;
            var y = year ?? now.Year;
            var m = month ?? now.Month;

            var model = new CalendarViewModel
            {
                Year = y,
                Month = m,
                Notes = _store.GetMonth(y, m)
                              .Select(x => new DayNote { Date = x.Date.ToDateTime(TimeOnly.MinValue), Note = x.Note })
                              .ToList()
            };

            // Add next 14 days notes
            var today = DateOnly.FromDateTime(DateTime.Today);
            var end = today.AddDays(14);
            model.UpcomingNotes = _store.GetAll()
                .Where(kv => kv.Key >= today && kv.Key < end)
                .OrderBy(kv => kv.Key)
                .Select(kv => new DayNote { Date = kv.Key.ToDateTime(TimeOnly.MinValue), Note = kv.Value })
                .ToList();

            return View(model);
        }


        // Returns notes for a given month as JSON
        [HttpGet]
        public IActionResult MonthNotes(int year, int month)
        {
            var notes = _store.GetMonth(year, month)
                              .Select(x => new { date = x.Date.ToString("yyyy-MM-dd"), note = x.Note });
            return Json(notes);
        }

        // Upsert note for a given date (yyyy-MM-dd)
        [HttpPost]
        public IActionResult SaveNote([FromForm] string date, [FromForm] string note)
        {
            if (!DateOnly.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                return BadRequest("Invalid date format.");
            _store.Upsert(d, note ?? string.Empty);
            return Ok(new { date, note });
        }

        // Delete note for a given date
        [HttpPost]
        public IActionResult DeleteNote([FromForm] string date)
        {
            if (!DateOnly.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                return BadRequest("Invalid date format.");
            _store.Remove(d);
            return Ok(new { date });
        }

        [HttpGet]
        public IActionResult UpcomingNotes()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var end = today.AddDays(14);

            var notes = _store.GetAll()
                .Where(kv => kv.Key >= today && kv.Key < end)
                .OrderBy(kv => kv.Key)
                .Select(kv => new { date = kv.Key.ToString("yyyy-MM-dd"), note = kv.Value })
                .ToList();

            return Json(notes);
        }


    }
}
