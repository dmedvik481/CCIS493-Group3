using System.Net;
using System.Net.Mail;


namespace HaircutBookingSystem.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpOptions _options;
        public SmtpEmailSender(SmtpOptions options)
        {
            _options = options;
        }


        public async Task SendAsync(string toEmail, string subject, string htmlBody)
        {
            using var client = new SmtpClient(_options.Host, _options.Port)
            {
                EnableSsl = _options.UseSsl,
                Credentials = new NetworkCredential(_options.Username, _options.Password)
            };


            var from = new MailAddress(_options.FromAddress, _options.FromName);
            var to = new MailAddress(toEmail);


            using var message = new MailMessage(from, to)
            {
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };


            await client.SendMailAsync(message);
        }
    }
}