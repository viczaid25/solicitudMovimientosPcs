using System.Threading.Tasks;

namespace solicitudMovimientosPcs.Services
{
    public interface IEmailService
    {
        Task SendAsync(string to, string subject, string htmlBody);
        Task SendAsync(string[] to, string subject, string htmlBody);
    }
}
