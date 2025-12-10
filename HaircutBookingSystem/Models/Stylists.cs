namespace HaircutBookingSystem.Models
{
    public class Stylist
    {
        public int StylistId { get; set; }
        public string StylistName { get; set; }

        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }
    }
}