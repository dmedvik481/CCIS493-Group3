using System.Threading.Tasks;


namespace HaircutBookingSystem.Services
{
    public interface IEmailSender
    {
        Task SendAsync(string toEmail, string subject, string htmlBody);
    }
}