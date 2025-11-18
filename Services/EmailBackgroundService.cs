using System.Net;
using System.Net.Mail;
using System.Threading.Channels;

namespace triage_backend.Services
{
    public class EmailBackgroundService : BackgroundService
    {
        private readonly Channel<MailMessage> _queue = Channel.CreateUnbounded<MailMessage>();

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (await _queue.Reader.WaitToReadAsync(stoppingToken))
            {
                var message = await _queue.Reader.ReadAsync(stoppingToken);
                try
                {
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
                    Console.WriteLine("❌ Error enviando correo en background: " + ex.Message);
                }
            }
        }

        public void Enqueue(MailMessage message)
        {
            _queue.Writer.TryWrite(message);
        }
    }
}
