namespace HaircutBookingSystem.Services
{
    public class SmtpOptions
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587; // typical STARTTLS
        public bool UseSsl { get; set; } = true; // true = SSL/TLS
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromAddress { get; set; } = string.Empty; // e.g. noreply@yourdomain.com
        public string FromName { get; set; } = "Haircut Booking";
    }
}