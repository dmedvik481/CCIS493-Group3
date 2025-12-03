using System.ComponentModel.DataAnnotations;


namespace HaircutBookingSystem.Models
{
    public class BookingRequest
    {
        [Required(ErrorMessage = "Please choose a service.")]
        [Display(Name = "Service")]
        public int ServiceId { get; set; }

        //double check for "stylistid"
        [Required(ErrorMessage = "Please choose a stylist.")]
        [Display(Name = "Preferred Stylist")]
        public int StylistId { get; set; }

        [Required(ErrorMessage = "Please choose a date.")]
        [DataType(DataType.Date)]
        [Display(Name = "Preferred Date")]
        public DateTime? Date { get; set; }


        [Required(ErrorMessage = "Please choose a time.")]
        [DataType(DataType.Time)]
        [Display(Name = "Preferred Time")]
        public TimeSpan? Time { get; set; }


        [Required]
        [StringLength(60)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;


        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;


        [Phone]
        [Display(Name = "Phone (optional)")]
        public string? Phone { get; set; }
    }
}