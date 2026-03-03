using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestionLlaves.Data;
using SistemaGestionLlaves.Models;

namespace SistemaGestionLlaves.Controllers;

[Route("Reportes")]
public class ReportesController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<ReportesController> _logger;

    public ReportesController(ApplicationDbContext db, ILogger<ReportesController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // GET: /Reportes o /Reportes/Index
    [HttpGet]
    [HttpGet("Index")]
    public IActionResult Index()
    {
        return View();
    }

    // API: /Reportes/GetPrestamosPorFecha
    [HttpGet("GetPrestamosPorFecha")]
    public async Task<IActionResult> GetPrestamosPorFecha([FromQuery] DateTime? inicio, [FromQuery] DateTime? fin)
    {
        try 
        {
            if (!inicio.HasValue || !fin.HasValue)
                return BadRequest(new { message = "Se requieren ambas fechas." });

            // Npgsql/Postgres requiere que las fechas sean UTC para compararlas con timestamptz
            var dInicio = DateTime.SpecifyKind(inicio.Value.Date, DateTimeKind.Utc);
            var dFin = DateTime.SpecifyKind(fin.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
            
            _logger.LogInformation("Consultando préstamos desde {Inicio} hasta {Fin}", dInicio, dFin);

            var prestamos = await _db.Prestamos
                .Include(p => p.Llave)
                .Include(p => p.Persona)
                .Where(p => p.FechaHoraPrestamo >= dInicio && p.FechaHoraPrestamo <= dFin)
                .OrderByDescending(p => p.FechaHoraPrestamo)
                .Select(p => new {
                    p.IdPrestamo,
                    p.FechaHoraPrestamo,
                    p.FechaHoraDevolucionReal,
                    p.Estado,
                    Llave = p.Llave.Codigo,
                    Persona = $"{p.Persona.Nombres} {p.Persona.Apellidos}"
                })
                .ToListAsync();

            return Ok(prestamos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetPrestamosPorFecha");
            return StatusCode(500, new { message = "Error interno al procesar el reporte.", detail = ex.Message });
        }
    }

    // API: /Reportes/GetHistorialPersona
    [HttpGet("GetHistorialPersona")]
    public async Task<IActionResult> GetHistorialPersona([FromQuery] int idPersona)
    {
        try
        {
            var historial = await _db.Prestamos
                .Include(p => p.Llave)
                .Where(p => p.IdPersona == idPersona)
                .OrderByDescending(p => p.FechaHoraPrestamo)
                .Select(p => new {
                    p.IdPrestamo,
                    p.FechaHoraPrestamo,
                    p.FechaHoraDevolucionReal,
                    p.Estado,
                    Llave = p.Llave.Codigo
                })
                .ToListAsync();

            return Ok(historial);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetHistorialPersona");
            return StatusCode(500, new { message = "Error al obtener historial." });
        }
    }

    // API: /Reportes/GetRankingLlaves
    [HttpGet("GetRankingLlaves")]
    public async Task<IActionResult> GetRankingLlaves()
    {
        try
        {
            var ranking = await _db.Prestamos
                .GroupBy(p => p.Llave.Codigo)
                .Select(g => new {
                    Llave = g.Key,
                    Cantidad = g.Count()
                })
                .OrderByDescending(x => x.Cantidad)
                .Take(10)
                .ToListAsync();

            return Ok(ranking);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetRankingLlaves");
            return StatusCode(500, new { message = "Error al calcular el ranking." });
        }
    }

    // Helper para cargar personas en los filtros del frontend
    [HttpGet("GetPersonas")]
    public async Task<IActionResult> GetPersonas()
    {
        try
        {
            // Simplificamos la consulta para ser más robustos con Postgres
            var personas = await _db.Personas
                .Where(p => p.Estado == "A")
                .OrderBy(p => p.Nombres)
                .ThenBy(p => p.Apellidos)
                .Select(p => new { 
                    p.IdPersona, 
                    Nombre = p.Nombres + " " + p.Apellidos 
                })
                .ToListAsync();
                
            return Ok(personas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetPersonas");
            return StatusCode(500, new { message = "Error al cargar la lista de personas.", detail = ex.Message });
        }
    }
}
