using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;

namespace triage_backend.Utilities
{
    public static class EmailUtility
    {
        private static readonly string senderEmail = "triageintelligent@gmail.com";
        private static readonly string senderPassword = "qdypntmqgcjxjqlm"; // clave de app Gmail

        public static void SendEmail(string toEmail, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
                throw new ArgumentException("Email address cannot be null or empty.", nameof(toEmail));

            using var smtp = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(senderEmail, senderPassword),
                EnableSsl = true,
                Timeout = 20000
            };

            using var message = new MailMessage
            {
                From = new MailAddress(senderEmail, "Intelligent Triage"),
                Subject = subject,
                IsBodyHtml = true
            };
            message.To.Add(toEmail);

            AlternateView htmlView = AlternateView.CreateAlternateViewFromString(body, null, MediaTypeNames.Text.Html);           

            // Obtener el logo embebido
            var assembly = typeof(EmailUtility).Assembly;
            using Stream? logoStream = assembly.GetManifestResourceStream("triage_backend.wwwroot.Images.logo.png");

            if (logoStream != null)
            {
                LinkedResource logo = new LinkedResource(logoStream, MediaTypeNames.Image.Jpeg)
                {
                    ContentId = "logo_triage",
                    TransferEncoding = TransferEncoding.Base64
                };

                htmlView.LinkedResources.Add(logo);
            }
            else
            {
                Console.WriteLine("⚠ No se pudo cargar el logo embebido.");
            }


            message.AlternateViews.Add(htmlView);

            smtp.Send(message);
        }
    }
}