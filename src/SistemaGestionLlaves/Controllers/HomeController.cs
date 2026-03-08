using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestionLlaves.Data;
using SistemaGestionLlaves.Models;

namespace SistemaGestionLlaves.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var model = new DashboardViewModel();

        // 1. KPIs
        model.TotalLlaves = await _context.Llaves.CountAsync(l => l.Estado != "I");
        model.PrestamosActivos = await _context.Prestamos.CountAsync(p => p.Estado == "A");
        
        var hoy = DateTime.UtcNow.Date;
        model.ReservasHoy = await _context.Reservas.CountAsync(r => r.FechaInicio >= hoy && r.FechaInicio < hoy.AddDays(1));
        
        model.PersonasRegistradas = await _context.Personas.CountAsync(p => p.Estado == "A");

        // 2. Actividad Reciente (Últimos 5 préstamos)
        model.ActividadReciente = await _context.Prestamos
            .Include(p => p.Persona)
            .Include(p => p.Llave).ThenInclude(l => l.Ambiente)
            .OrderByDescending(p => p.FechaHoraPrestamo)
            .Take(5)
            .Select(p => new ActividadRecienteDto
            {
                PersonaNombre = p.Persona.Nombres + " " + p.Persona.Apellidos,
                PersonaTipo = "-", // Podrías mapear el tipo si existiera en Persona
                LlaveCodigo = p.Llave.Codigo,
                AmbienteNombre = p.Llave.Ambiente != null ? p.Llave.Ambiente.Nombre : "-",
                Fecha = p.FechaHoraPrestamo,
                Estado = p.Estado,
                TipoAccion = "Préstamo"
            })
            .ToListAsync();

        // 3. Gráfico Dona: Uso por Tipo de Ambiente
        model.UsoPorTipo = await _context.Ambientes
            .Include(a => a.TipoAmbiente)
            .GroupBy(a => a.TipoAmbiente.NombreTipo)
            .Select(g => new UsoTipoAmbienteDto
            {
                Tipo = g.Key,
                Cantidad = g.Count()
            })
            .ToListAsync();

        // 4. Gráfico Histórico: Últimos 6 meses
        var seisMesesAtras = DateTime.UtcNow.AddMonths(-5);
        seisMesesAtras = new DateTime(seisMesesAtras.Year, seisMesesAtras.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var prestamosHistoricos = await _context.Prestamos
            .Where(p => p.FechaHoraPrestamo >= seisMesesAtras)
            .ToListAsync();

        model.HistoricoPrestamos = prestamosHistoricos
            .GroupBy(p => new { p.FechaHoraPrestamo.Year, p.FechaHoraPrestamo.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new HistoricoChartDto
            {
                Mes = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yy"),
                Cantidad = g.Count()
            })
            .ToList();

        // 5. Alertas Críticas (Préstamos vencidos hace más de 24h)
        var limiteAlerta = DateTime.UtcNow.AddDays(-1);
        model.AlertasCriticas = await _context.Prestamos
            .Include(p => p.Persona)
            .Include(p => p.Llave)
            .Where(p => p.Estado == "A" && p.FechaHoraDevolucionEsperada < limiteAlerta)
            .OrderBy(p => p.FechaHoraDevolucionEsperada)
            .Select(p => new AlertaCriticaDto
            {
                IdPrestamo = p.IdPrestamo,
                PersonaNombre = p.Persona.Nombres + " " + p.Persona.Apellidos,
                LlaveCodigo = p.Llave.Codigo,
                FechaVencimiento = p.FechaHoraDevolucionEsperada.GetValueOrDefault(),
                HorasRetraso = (int)(DateTime.UtcNow - p.FechaHoraDevolucionEsperada.GetValueOrDefault()).TotalHours
            })
            .ToListAsync();

        return View(model);
    }

    public IActionResult Error()
    {
        return View();
    }
}
