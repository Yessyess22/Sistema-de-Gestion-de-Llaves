using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<Ambiente>>> ObtenerTodos()
    {
        try
        {
            _logger.LogInformation("Obteniendo todos los ambientes");
            var ambientes = await _db.Ambientes
                .Include(a => a.TipoAmbiente)
                .OrderBy(a => a.Nombre)
                .ToListAsync();

            return Ok(ambientes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener ambientes");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { mensaje = "Error al obtener los ambientes", detalle = ex.Message });
        }
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Ambiente>> ObtenerPorId(int id)
    {
        if (id <= 0)
        {
            _logger.LogWarning("Intento de acceso con ID inválido: {id}", id);
            return BadRequest(new { mensaje = "El ID debe ser mayor a cero" });
        }

        try
        {
            _logger.LogInformation("Obteniendo ambiente con ID: {id}", id);
            var ambiente = await _db.Ambientes
                .Include(a => a.TipoAmbiente)
                .FirstOrDefaultAsync(a => a.IdAmbiente == id);

            if (ambiente == null)
            {
                return NotFound(new { mensaje = $"No se encontró ambiente con ID {id}" });
            }

            return Ok(ambiente);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener ambiente con ID: {id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { mensaje = "Error al obtener el ambiente", detalle = ex.Message });
        }
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Ambiente>> Crear(Ambiente ambiente)
    {
        if (ambiente == null)
        {
            _logger.LogWarning("Intento de crear ambiente con datos nulos");
            return BadRequest(new { mensaje = "El ambiente no puede estar vacío" });
        }

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Intento de crear ambiente con datos inválidos: {@ambiente}", ambiente);
            return BadRequest(ModelState);
        }

        try
        {
            ambiente.Codigo = ambiente.Codigo.Trim().ToUpper();
            ambiente.Nombre = ambiente.Nombre.Trim();

            var codigoExiste = await _db.Ambientes.AnyAsync(a => a.Codigo == ambiente.Codigo);
            if (codigoExiste)
            {
                return Conflict(new { mensaje = "Ya existe un ambiente con ese código" });
            }

            var tipoExiste = await _db.TiposAmbiente.AnyAsync(t => t.IdTipo == ambiente.IdTipo);
            if (!tipoExiste)
            {
                return BadRequest(new { mensaje = "El tipo de ambiente no existe" });
            }

            _db.Ambientes.Add(ambiente);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Ambiente creado con ID: {id}", ambiente.IdAmbiente);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = ambiente.IdAmbiente }, ambiente);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear ambiente");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { mensaje = "Error al crear el ambiente", detalle = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Ambiente>> Actualizar(int id, Ambiente ambiente)
    {
        if (id <= 0)
        {
            _logger.LogWarning("Intento de actualizar con ID inválido: {id}", id);
            return BadRequest(new { mensaje = "El ID debe ser mayor a cero" });
        }

        if (ambiente == null)
        {
            _logger.LogWarning("Intento de actualizar ambiente con datos nulos");
            return BadRequest(new { mensaje = "El ambiente no puede estar vacío" });
        }

        if (id != ambiente.IdAmbiente)
        {
            return BadRequest(new { mensaje = "El ID de la URL no coincide con el del ambiente" });
        }

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Intento de actualizar ambiente con datos inválidos");
            return BadRequest(ModelState);
        }

        try
        {
            var existente = await _db.Ambientes.FirstOrDefaultAsync(a => a.IdAmbiente == id);
            if (existente == null)
            {
                return NotFound(new { mensaje = $"No se encontró ambiente con ID {id}" });
            }

            var codigo = ambiente.Codigo.Trim().ToUpper();
            var codigoDuplicado = await _db.Ambientes
                .AnyAsync(a => a.Codigo == codigo && a.IdAmbiente != id);
            if (codigoDuplicado)
            {
                return Conflict(new { mensaje = "Ya existe un ambiente con ese código" });
            }

            var tipoExiste = await _db.TiposAmbiente.AnyAsync(t => t.IdTipo == ambiente.IdTipo);
            if (!tipoExiste)
            {
                return BadRequest(new { mensaje = "El tipo de ambiente no existe" });
            }

            existente.Codigo = codigo;
            existente.Nombre = ambiente.Nombre.Trim();
            existente.Ubicacion = ambiente.Ubicacion?.Trim();
            existente.IdTipo = ambiente.IdTipo;
            existente.Estado = ambiente.Estado;

            await _db.SaveChangesAsync();
            _logger.LogInformation("Ambiente actualizado con ID: {id}", id);
            return Ok(existente);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar ambiente");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { mensaje = "Error al actualizar el ambiente", detalle = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Eliminar(int id)
    {
        if (id <= 0)
        {
            _logger.LogWarning("Intento de eliminar con ID inválido: {id}", id);
            return BadRequest(new { mensaje = "El ID debe ser mayor a cero" });
        }

        try
        {
            _logger.LogInformation("Eliminando ambiente con ID: {id}", id);
            var ambiente = await _db.Ambientes.FirstOrDefaultAsync(a => a.IdAmbiente == id);
            if (ambiente == null)
            {
                return NotFound(new { mensaje = $"No se encontró ambiente con ID {id}" });
            }

            _db.Ambientes.Remove(ambiente);
            await _db.SaveChangesAsync();
            return NoContent();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(ex, "No se pudo eliminar ambiente con ID {id} por relaciones existentes", id);
            return Conflict(new
            {
                mensaje = "No se puede eliminar el ambiente porque tiene registros relacionados"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar ambiente");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { mensaje = "Error al eliminar el ambiente", detalle = ex.Message });
        }
    }
}
