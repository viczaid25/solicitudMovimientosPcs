using System.Threading.Tasks;

namespace solicitudMovimientosPcs.Services
{
    public interface IEmailService
    {
        Task SendAsync(string to, string subject, string html);
        Task SendAsync(IEnumerable<string> to, string subject, string html);
    }
}
