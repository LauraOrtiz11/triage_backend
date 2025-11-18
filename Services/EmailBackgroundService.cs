using System.Net;
using System.Net.Mail;
using System.Threading.Channels;

namespace triage_backend.Services
{
    public record EmailQueueItem(string To, string Subject, string HtmlBody);

    public class EmailBackgroundService : BackgroundService
    {
        private readonly Channel<EmailQueueItem> _queue = Channel.CreateUnbounded<EmailQueueItem>();

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (await _queue.Reader.WaitToReadAsync(stoppingToken))
            {
                var email = await _queue.Reader.ReadAsync(stoppingToken);

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

                    await smtp.SendMailAsync(message, stoppingToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("❌ Error enviando correo: " + ex.Message);
                }
            }
        }

        public void Enqueue(string to, string subject, string htmlBody)
        {
            _queue.Writer.TryWrite(new EmailQueueItem(to, subject, htmlBody));
        }
    }
}
