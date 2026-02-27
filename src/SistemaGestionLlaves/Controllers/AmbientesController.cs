using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestionLlaves.Data;
using SistemaGestionLlaves.Models;

namespace SistemaGestionLlaves.Controllers
{
    public class AmbientesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AmbientesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // LISTAR
        public async Task<IActionResult> Index()
    {
        var ambientes = await _context.Ambientes
        .Include(a => a.TipoAmbiente)
        .ToListAsync();

         return View(ambientes);
   }

   public async Task<IActionResult> Details(int id)
{
    var ambiente = await _context.Ambientes
        .Include(a => a.TipoAmbiente)
        .FirstOrDefaultAsync(a => a.IdAmbiente == id);

    if (ambiente == null)
        return NotFound();

    return View(ambiente);
}

        // CREAR GET
        public async Task<IActionResult> Create()
{
    ViewBag.Tipos = new SelectList(
        await _context.TiposAmbiente.ToListAsync(),
        "IdTipo",
        "NombreTipo"   // 👈 CORREGIDO
    );

    return View();
}


        // CREAR POST
        [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(Ambiente ambiente)
{
    if (ModelState.IsValid)
    {
        _context.Add(ambiente);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // Si hay error, volver a cargar el combo
    ViewBag.Tipos = new SelectList(
        await _context.TiposAmbiente.ToListAsync(),
        "IdTipo",
        "NombreTipo",
        ambiente.IdTipo
    );

    return View(ambiente);
}

        // EDITAR POST
       [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(int id, Ambiente ambiente)
{
    if (id != ambiente.IdAmbiente)
        return NotFound();

    if (ModelState.IsValid)
    {
        try
        {
            _context.Update(ambiente);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Ambientes.Any(e => e.IdAmbiente == ambiente.IdAmbiente))
                return NotFound();
            else
                throw;
        }

        return RedirectToAction(nameof(Index));
    }

    ViewBag.Tipos = new SelectList(
        await _context.TiposAmbiente.ToListAsync(),
        "IdTipo",
        "NombreTipo",
        ambiente.IdTipo
    );

    return View(ambiente);
}
        // EDITAR
       public async Task<IActionResult> Edit(int id)
{
    var ambiente = await _context.Ambientes.FindAsync(id);

    if (ambiente == null)
        return NotFound();

    ViewBag.Tipos = new SelectList(await _context.TiposAmbiente.ToListAsync(), "IdTipo", "NombreTipo", ambiente.IdTipo);

    return View(ambiente);
}

    // DELETE GET
public async Task<IActionResult> Delete(int id)
{
    var ambiente = await _context.Ambientes
                                 .Include(a => a.TipoAmbiente)
                                 .FirstOrDefaultAsync(a => a.IdAmbiente == id);

    if (ambiente == null)
        return NotFound();

    return View(ambiente);
}

// DELETE POST
[HttpPost, ActionName("Delete")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> DeleteConfirmed(int id)
{
    var ambiente = await _context.Ambientes.FindAsync(id);

    if (ambiente != null)
    {
        _context.Ambientes.Remove(ambiente);
        await _context.SaveChangesAsync();
    }

    return RedirectToAction(nameof(Index));
}


    }

    
}
