using System.Threading.Tasks;

namespace SistemaGestionLlaves.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendNotificationAsync(string to, string title, string message);
    }
}
