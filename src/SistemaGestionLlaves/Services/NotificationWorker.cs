using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SistemaGestionLlaves.Data;

namespace SistemaGestionLlaves.Services
{
    public class NotificationWorker : BackgroundService
    {
        private readonly ILogger<NotificationWorker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private DateTime _lastDailyReportRun = DateTime.MinValue;

        public NotificationWorker(ILogger<NotificationWorker> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("NotificationWorker iniciado.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                        await ProcesarAlertasVencimientoAsync(context, emailService);
                        await ProcesarAlertasReservasAsync(context, emailService);
                        await ProcesarReporteDiarioAsync(context, emailService);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en el ciclo de ejecución de NotificationWorker.");
                }

                // Esperar 10 minutos antes del siguiente ciclo
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
        }

        private async Task ProcesarAlertasVencimientoAsync(ApplicationDbContext context, IEmailService emailService)
        {
            _logger.LogInformation("Verificando préstamos por vencer...");

            // Préstamos que vencen en la próxima hora (60 min) y no han sido notificados
            // Nota: Para evitar spam, necesitaríamos una marca de 'Notificado' en el préstamo.
            // De momento, simplemente buscamos los que están en el rango.
            var unaHoraDespues = DateTime.UtcNow.AddHours(1);
            
            var prestamosPorVencer = await context.Prestamos
                .Include(p => p.Persona)
                .Include(p => p.Llave)
                .Where(p => p.Estado == "A" 
                            && p.FechaHoraDevolucionEsperada <= unaHoraDespues 
                            && p.FechaHoraDevolucionEsperada > DateTime.UtcNow)
                .ToListAsync();

            foreach (var p in prestamosPorVencer)
            {
                if (!string.IsNullOrEmpty(p.Persona.Correo))
                {
                    await emailService.SendNotificationAsync(
                        p.Persona.Correo,
                        "Recordatorio de Devolución de Llave",
                        $"Hola {p.Persona.Nombres}, tu préstamo de la llave {p.Llave.Codigo} vence en menos de una hora ({p.FechaHoraDevolucionEsperada:HH:mm}). Por favor, procede a su devolución."
                    );
                }
            }
        }

        private async Task ProcesarAlertasReservasAsync(ApplicationDbContext context, IEmailService emailService)
        {
            _logger.LogInformation("Verificando reservas próximas...");

            // Reservas que inician en los próximos 30 minutos
            var treintaMinutos = DateTime.UtcNow.AddMinutes(30);
            
            var reservasProximas = await context.Reservas
                .Include(r => r.Persona)
                .Include(r => r.Llave).ThenInclude(l => l.Ambiente)
                .Where(r => r.Estado == "A" 
                            && r.FechaInicio <= treintaMinutos 
                            && r.FechaInicio > DateTime.UtcNow)
                .ToListAsync();

            foreach (var r in reservasProximas)
            {
                var ambienteNombre = r.Llave.Ambiente != null ? r.Llave.Ambiente.Nombre : "Sin Ambiente";
                _logger.LogInformation("RESERVA PRÓXIMA: La reserva para {Ambiente} por {Persona} inicia en breve ({Hora}).", 
                    ambienteNombre, r.Persona.NombreCompleto, r.FechaInicio.ToString("HH:mm"));

                if (!string.IsNullOrEmpty(r.Persona.Correo))
                {
                    await emailService.SendNotificationAsync(
                        r.Persona.Correo,
                        "Tu reserva está por iniciar",
                        $"Hola {r.Persona.Nombres}, recordatorio de que tu reserva para el ambiente {ambienteNombre} inicia en menos de 30 minutos ({r.FechaInicio:HH:mm})."
                    );
                }
            }
        }

        private async Task ProcesarReporteDiarioAsync(ApplicationDbContext context, IEmailService emailService)
        {
            // Ejecutar una vez al día a las 08:00 AM (aproximadamente)
            var ahora = DateTime.Now;
            if (ahora.Hour == 8 && _lastDailyReportRun.Date != ahora.Date)
            {
                _logger.LogInformation("Generando reporte diario de morosidad...");

                var vencidos = await context.Prestamos
                    .Include(p => p.Persona)
                    .Include(p => p.Llave)
                    .Where(p => p.Estado == "A" && p.FechaHoraDevolucionEsperada < DateTime.UtcNow)
                    .ToListAsync();

                if (vencidos.Any())
                {
                    // Enviar a un correo de administración (configurado en appsettings)
                    var adminEmail = "admin@upds.edu.bo";
                    var body = $"Se han detectado {vencidos.Count} llaves no devueltas al inicio de la jornada.";
                    await emailService.SendNotificationAsync(adminEmail, "Resumen Diario de Morosidad", body);
                }

                _lastDailyReportRun = ahora;
            }
        }
    }
}
