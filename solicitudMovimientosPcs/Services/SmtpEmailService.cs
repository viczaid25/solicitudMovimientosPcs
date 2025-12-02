using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Collections.Generic; // <-- IMPORTANTE para IEnumerable<string>

namespace solicitudMovimientosPcs.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly EmailSettings _cfg;

        public SmtpEmailService(IOptions<EmailSettings> cfg)
        {
            _cfg = cfg.Value;
        }

        // Conveniencia: un solo destinatario
        public Task SendAsync(string to, string subject, string html)
            => SendAsync((IEnumerable<string>)new[] { to }, subject, html);

        // Firma EXACTA que pide la interfaz
        public async Task SendAsync(IEnumerable<string> to, string subject, string html)
        {
            using var msg = new MailMessage
            {
                From = new MailAddress(_cfg.FromEmail),
                Subject = subject,
                Body = html,
                IsBodyHtml = true
            };

            foreach (var t in to)
            {
                if (!string.IsNullOrWhiteSpace(t))
                    msg.To.Add(t.Trim());
            }

            using var client = new SmtpClient(_cfg.SmtpServer, _cfg.SmtpPort)
            {
                EnableSsl = _cfg.EnableSsl,
                UseDefaultCredentials = true,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            await client.SendMailAsync(msg);
        }
    }
}
