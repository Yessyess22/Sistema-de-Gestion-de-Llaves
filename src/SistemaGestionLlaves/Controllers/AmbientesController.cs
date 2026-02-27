using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestionLlaves.Data;
using SistemaGestionLlaves.Models;

namespace SistemaGestionLlaves.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AmbientesController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<AmbientesController> _logger;

    public AmbientesController(ApplicationDbContext db, ILogger<AmbientesController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Ambiente>>> ObtenerTodos()
    {
        try
        {
            var ambientes = await _db.Ambientes
                .Include(a => a.TipoAmbiente)
                .ToListAsync();
            return Ok(ambientes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener ambientes");
            return StatusCode(500, new { mensaje = "Error interno" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Ambiente>> ObtenerPorId(int id)
    {
        var ambiente = await _db.Ambientes
            .Include(a => a.TipoAmbiente)
            .FirstOrDefaultAsync(a => a.IdAmbiente == id);

        if (ambiente == null)
            return NotFound();

        return Ok(ambiente);
    }
}
