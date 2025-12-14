using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace CalendarDemo.Services
{
    // Thread-safe per-app in-memory store
    public class InMemoryNoteStore
    {
        private readonly ConcurrentDictionary<DateOnly, string> _notes = new();

        public IReadOnlyDictionary<DateOnly, string> GetAll() => _notes;

        public string? Get(DateOnly date) => _notes.TryGetValue(date, out var note) ? note : null;

        public void Upsert(DateOnly date, string note)
        {
            _notes.AddOrUpdate(date, note, (_, __) => note);
        }

        public void Remove(DateOnly date)
        {
            _notes.TryRemove(date, out _);
        }

        public IEnumerable<(DateOnly Date, string Note)> GetMonth(int year, int month)
        {
            return _notes.Where(kv => kv.Key.Year == year && kv.Key.Month == month)
                         .Select(kv => (kv.Key, kv.Value))
                         .OrderBy(x => x.Key);
        }
    }
}
