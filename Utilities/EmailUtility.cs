using System.Net.Mail;

namespace triage_backend.Utilities
{
    public static class EmailUtility
    {
        public static MailMessage BuildEmail(string to, string subject, string body)
        {
            var message = new MailMessage
            {
                From = new MailAddress("triageintelligent@gmail.com", "Intelligent Triage"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            message.To.Add(to);
            return message;
        }
    }
}
