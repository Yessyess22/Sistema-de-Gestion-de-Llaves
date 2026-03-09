using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaGestionLlaves.Data;

namespace SistemaGestionLlaves.Controllers;

/// <summary>
/// Controlador MVC para las vistas de Reservas.
/// Rutas: /Reservas
/// </summary>
public class ReservasController : Controller
{
    private readonly ApplicationDbContext _context;

    public ReservasController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /Reservas
    public IActionResult Index() => View();

    // GET: /Reservas/Crear
    public async Task<IActionResult> Crear()
    {
        ViewData["Personas"] = new SelectList(
            await _context.Personas
                .Where(p => p.Estado == "A")
                .OrderBy(p => p.Apellidos).ThenBy(p => p.Nombres)
                .Select(p => new { p.IdPersona, Nombre = p.Apellidos + ", " + p.Nombres + " (" + p.Ci + ")" })
                .ToListAsync(),
            "IdPersona", "Nombre");

        ViewData["Llaves"] = new SelectList(
            await _context.Llaves
                .Include(l => l.Ambiente)
                .Where(l => l.Estado == "D" || l.Estado == "R")
                .OrderBy(l => l.Codigo)
                .Select(l => new { l.IdLlave, Descripcion = l.Codigo + " — " + l.Ambiente!.Nombre })
                .ToListAsync(),
            "IdLlave", "Descripcion");

        return View();
    }
}
