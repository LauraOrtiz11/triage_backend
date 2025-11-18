using System.Net;
using System.Net.Mail;
using System.Threading.Channels;

namespace triage_backend.Services
{
    public record EmailQueueItem(string To, string Subject, string HtmlBody);

    public class EmailBackgroundService : BackgroundService
    {
        private readonly Channel<EmailQueueItem> _queue = Channel.CreateUnbounded<EmailQueueItem>();

        public EmailBackgroundService()
        {
            Console.WriteLine("⚡ EmailBackgroundService CONSTRUCTOR ejecutado.");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("⚡ EmailBackgroundService iniciado correctamente. Esperando correos...");

            while (await _queue.Reader.WaitToReadAsync(stoppingToken))
            {
                Console.WriteLine("📦 Detected items in queue… processing...");

                var email = await _queue.Reader.ReadAsync(stoppingToken);

                Console.WriteLine($"🚀 Procesando envío de correo:");
                Console.WriteLine($"   👉 Para: {email.To}");
                Console.WriteLine($"   👉 Asunto: {email.Subject}");

                try
                {
                    using var message = new MailMessage
                    {
                        From = new MailAddress("triageintelligent@gmail.com", "Intelligent Triage"),
                        Subject = email.Subject,
                        Body = email.HtmlBody,
                        IsBodyHtml = true
                    };

                    message.To.Add(email.To);

                    using var smtp = new SmtpClient("smtp.gmail.com")
                    {
                        Port = 587,
                        Credentials = new NetworkCredential("triageintelligent@gmail.com", "qdypntmqgcjxjqlm"),
                        EnableSsl = true,
                        Timeout = 20000
                    };

                    Console.WriteLine("📨 Enviando correo vía SMTP...");
                    await smtp.SendMailAsync(message, stoppingToken);
                    Console.WriteLine("✅ CORREO ENVIADO EXITOSAMENTE");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("❌ ERROR EN ENVÍO DE CORREO:");
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        public void Enqueue(string to, string subject, string htmlBody)
        {
            Console.WriteLine($"📩 Encolando correo: {to}, Asunto: {subject}");

            bool success = _queue.Writer.TryWrite(new EmailQueueItem(to, subject, htmlBody));

            if (success)
                Console.WriteLine("📥 Correo encolado correctamente.");
            else
                Console.WriteLine("❌ ERROR: No se pudo encolar el correo.");
        }
    }
}
