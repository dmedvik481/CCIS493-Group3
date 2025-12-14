using System;
using System.Collections.Generic;

namespace CalendarDemo.Models
{
    public class DayNote
    {
        public DateTime Date { get; set; }
        public string Note { get; set; } = string.Empty;
    }

    public class CalendarViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public List<DayNote> Notes { get; set; } = new();
        public List<DayNote> UpcomingNotes { get; set; } = new(); // new
    }

}
