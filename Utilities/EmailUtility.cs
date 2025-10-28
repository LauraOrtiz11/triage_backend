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

            // ✅ Ruta segura y sin advertencias
            string? baseDir = AppContext.BaseDirectory;
            DirectoryInfo? dir = new DirectoryInfo(baseDir);
            string? projectRoot = dir?.Parent?.Parent?.Parent?.FullName;

            if (string.IsNullOrEmpty(projectRoot))
                throw new InvalidOperationException("No se pudo determinar la ruta raíz del proyecto.");

            string imagePath = Path.Combine(projectRoot, "wwwroot", "Images", "logo.png");

            if (File.Exists(imagePath))
            {
                LinkedResource logo = new LinkedResource(imagePath, MediaTypeNames.Image.Jpeg)
                {
                    ContentId = "logo_triage",
                    TransferEncoding = TransferEncoding.Base64
                };
                htmlView.LinkedResources.Add(logo);
            }
            else
            {
                Console.WriteLine($"⚠ No se encontró el logo en: {imagePath}");
            }

            message.AlternateViews.Add(htmlView);

            smtp.Send(message);
        }
    }
}