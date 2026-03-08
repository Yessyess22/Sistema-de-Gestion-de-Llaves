using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace SistemaGestionLlaves.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly ILogger<SmtpEmailService> _logger;
        private readonly IConfiguration _configuration;

        public SmtpEmailService(ILogger<SmtpEmailService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            // En desarrollo, simulamos el envío en los logs.
            // En producción, aquí se usaría MailKit o SmtpClient.
            _logger.LogInformation("SIMULACIÓN EMAIL: Para: {To}, Asunto: {Subject}, Cuerpo: {Body}", to, subject, body);
            
            // Simular retraso de red
            await Task.Delay(100);
        }

        public async Task SendNotificationAsync(string to, string title, string message)
        {
            var body = $@"
                <h3>{title}</h3>
                <p>{message}</p>
                <hr>
                <small>Este es un mensaje automático del Sistema de Gestión de Llaves.</small>
            ";

            await SendEmailAsync(to, title, body);
        }
    }
}
