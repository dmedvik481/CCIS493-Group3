using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HaircutBookingSystem.Models
{
    public class Stylist
    {
        [Key]
        [Column("StylistID")] // maps to the existing DB column name
        public int StylistId { get; set; }

        [Required]
        public string StylistName { get; set; } = "";

        // Optional columns (make nullable so NULLs don’t crash EF)
        public string? Bio { get; set; }
        public string? Specialty { get; set; }

        // If you have this column in DB, keep it nullable
        public string? UserId { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
