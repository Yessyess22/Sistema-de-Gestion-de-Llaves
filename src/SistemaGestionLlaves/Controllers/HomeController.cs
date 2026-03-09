using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestionLlaves.Data;

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

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetDashboardData()
    {
        var totalLlaves = await _context.Llaves.CountAsync(l => l.Estado != "E"); // E usually for Eliminado or Inactivo
        var prestamosActivos = await _context.Prestamos.CountAsync(p => p.Estado == "A");

        var ambientesTotal = await _context.Ambientes.CountAsync(a => a.Estado == "A");
        var llavesPrestadas = await _context.Prestamos
            .Where(p => p.Estado == "A")
            .Select(p => p.IdLlave)
            .Distinct()
            .CountAsync();
        
        var actividadReciente = await _context.Prestamos
            .Include(p => p.Persona)
            .Include(p => p.Llave).ThenInclude(l => l.Ambiente)
            .OrderByDescending(p => p.FechaHoraPrestamo)
            .Take(5)
            .Select(p => new {
                usuario = $"{p.Persona.Nombres} {p.Persona.Apellidos}",
                rol = "Usuario", 
                llave = p.Llave.Codigo,
                ambiente = p.Llave.Ambiente.Nombre,
                fecha = p.FechaHoraPrestamo.ToString("dd/MM/yyyy HH:mm"),
                estado = p.Estado == "A" ? "Prestado" : "Devuelto"
            })
            .ToListAsync();

        var alertas = await _context.AlertasNotificaciones
            .Include(a => a.Llave)
            .Where(a => !a.Leida)
            .OrderByDescending(a => a.FechaGenerada)
            .Take(3)
            .Select(a => new {
                titulo = a.TipoAlerta,
                mensaje = a.Mensaje
            })
            .ToListAsync();

        return Json(new {
            totalLlaves,
            prestamosActivos,
            ambientesUsados = llavesPrestadas,
            ambientesTotal,
            actividadReciente,
            alertas,
            currentTime = DateTime.Now.ToString("dd 'de' MMMM, HH:mm")
        });
    }

    public IActionResult Error()
    {
        return View();
    }
}
