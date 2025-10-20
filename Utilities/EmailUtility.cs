using System.Net;
using System.Net.Mail;

namespace triage_backend.Utilities
{
    public static class EmailUtility
    {
        public static void SendEmail(string toEmail, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
                throw new ArgumentException("Email address cannot be null or empty.", nameof(toEmail));

            using var smtp = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("dcakesnot@gmail.com", "buqkdatqrdmmnjrg"),
                EnableSsl = true
            };

            using var message = new MailMessage("dcakesnot@gmail.com", toEmail, subject, body)
            {
                IsBodyHtml = true
            };

            smtp.Send(message);
        }
    }
}
