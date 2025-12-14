using System.ComponentModel.DataAnnotations;


namespace HaircutBookingSystem.Models
{
    public class Service
    {
        public int Id { get; set; }


        [Required]
        [Display(Name = "Service Name")]
        public string Name { get; set; } = string.Empty;


        // Optional: show a price on the form
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal? Price { get; set; }
    }
}