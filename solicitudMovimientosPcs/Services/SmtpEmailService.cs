using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace solicitudMovimientosPcs.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly EmailSettings _cfg;

        public SmtpEmailService(IOptions<EmailSettings> cfg)
        {
            _cfg = cfg.Value;
        }

        public Task SendAsync(string to, string subject, string htmlBody)
            => SendAsync(new[] { to }, subject, htmlBody);

        public async Task SendAsync(string[] to, string subject, string htmlBody)
        {
            using var msg = new MailMessage
            {
                From = new MailAddress(_cfg.FromEmail),
                Subject = subject,
                Body = htmlBody,
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
                // Para relay por IP en O365 NO usar credenciales.
                UseDefaultCredentials = true,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            // Si tu relay requiere TLS explícito y falla con SSL=false, puedes intentar:
            // client.EnableSsl = true;  // sólo si tu conector lo exige

            await client.SendMailAsync(msg);
        }
    }
}
