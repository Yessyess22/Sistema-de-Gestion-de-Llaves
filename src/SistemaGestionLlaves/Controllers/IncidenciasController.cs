using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaGestionLlaves.Data;
using SistemaGestionLlaves.Models;

namespace SistemaGestionLlaves.Controllers
{
    [Authorize]
    public class IncidenciasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public IncidenciasController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var incidencias = await _context.Incidencias
                .Include(i => i.Llave)
                    .ThenInclude(l => l.Ambiente)
                .OrderByDescending(i => i.FechaReporte)
                .ToListAsync();

            ViewBag.Llaves = new SelectList(await _context.Llaves
                .OrderBy(l => l.Codigo)
                .Select(l => new { l.IdLlave, Texto = $"{l.Codigo} - {(l.Ambiente != null ? l.Ambiente.Nombre : "Sin Ambiente")}" })
                .ToListAsync(), "IdLlave", "Texto");

            return View(incidencias);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(Incidencia incidencia)
        {
            if (ModelState.IsValid)
            {
                incidencia.FechaReporte = DateTime.UtcNow;
                incidencia.Estado = "A"; // Abierta

                _context.Add(incidencia);

                // Cambiar estado de la llave a Mantenimiento
                var llave = await _context.Llaves.FindAsync(incidencia.IdLlave);
                if (llave != null)
                {
                    llave.Estado = "M"; // Mantenimiento
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Incidencia registrada correctamente. La llave ha sido bloqueada.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Error al registrar la incidencia. Verifique los datos.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Resolver(int id, string notas)
        {
            var incidencia = await _context.Incidencias.FindAsync(id);
            if (incidencia == null) return NotFound();

            incidencia.Estado = "R"; // Resuelta
            incidencia.FechaResolucion = DateTime.UtcNow;
            incidencia.NotasResolucion = notas;

            // Cambiar estado de la llave de vuelta a Disponible (si no está prestada)
            var llave = await _context.Llaves.FindAsync(incidencia.IdLlave);
            if (llave != null)
            {
                // Verificar si hay otras incidencias abiertas para esta misma llave
                var otrasAbiertas = await _context.Incidencias
                    .AnyAsync(i => i.IdLlave == llave.IdLlave && i.Estado == "A" && i.IdIncidencia != id);

                if (!otrasAbiertas)
                {
                    llave.Estado = "D"; // Disponible
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Incidencia resuelta. La llave vuelve a estar habilitada.";
            return RedirectToAction(nameof(Index));
        }
    }
}
