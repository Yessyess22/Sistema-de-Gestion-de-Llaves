using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestionLlaves.Data;
using SistemaGestionLlaves.Models;

namespace SistemaGestionLlaves.Controllers;

// ─────────────────────────────────────────────────────────────
//  DTOs de entrada
// ─────────────────────────────────────────────────────────────

/// <summary>Cuerpo para crear una reserva.</summary>
public record ReservaRequest(
    int IdLlave,
    int IdPersona,
    int IdUsuario,
    DateTime FechaInicio,
    DateTime FechaFin
);

// ─────────────────────────────────────────────────────────────
//  Controlador
// ─────────────────────────────────────────────────────────────

/// <summary>
/// API REST para la gestión de Reservas.
/// Base URL: /api/reservas
/// </summary>
[ApiController]
[Route("api/reservas")]
[Produces("application/json")]
public class ReservasApiController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<ReservasApiController> _logger;

    public ReservasApiController(ApplicationDbContext db, ILogger<ReservasApiController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // ── GET /api/reservas ────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? estado)
    {
        var query = _db.Reservas
            .Include(r => r.Llave).ThenInclude(l => l.Ambiente)
            .Include(r => r.Persona)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(estado))
            query = query.Where(r => r.Estado == estado.ToUpper());

        var reservas = await query
            .OrderByDescending(r => r.FechaInicio)
            .Select(r => new
            {
                r.IdReserva,
                r.IdLlave,
                r.IdPersona,
                r.FechaInicio,
                r.FechaFin,
                r.Estado,
                Llave = new
                {
                    r.Llave.IdLlave,
                    r.Llave.Codigo,
                    r.Llave.Estado,
                    Ambiente = r.Llave.Ambiente == null ? null : new { r.Llave.Ambiente.Nombre }
                },
                Persona = new
                {
                    r.Persona.IdPersona,
                    r.Persona.Ci,
                    Nombre = $"{r.Persona.Nombres} {r.Persona.Apellidos}"
                }
            })
            .ToListAsync();

        return Ok(reservas);
    }

    // ── POST /api/reservas ───────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] ReservaRequest dto)
    {
        if (dto.IdLlave <= 0)
            return BadRequest(new ApiResponse(false, "El Id de llave es obligatorio."));

        if (dto.IdPersona <= 0)
            return BadRequest(new ApiResponse(false, "El Id de persona es obligatorio."));

        if (dto.IdUsuario <= 0)
            return BadRequest(new ApiResponse(false, "El Id de usuario (operador) es obligatorio."));

        if (dto.FechaInicio >= dto.FechaFin)
            return BadRequest(new ApiResponse(false, "La fecha de inicio debe ser anterior a la fecha de fin."));

        if (dto.FechaInicio <= DateTime.UtcNow)
            return BadRequest(new ApiResponse(false, "La fecha de inicio debe ser posterior a la fecha actual."));

        var llave = await _db.Llaves.FindAsync(dto.IdLlave);
        if (llave == null)
            return NotFound(new ApiResponse(false, $"Llave con Id={dto.IdLlave} no encontrada."));

        if (llave.Estado == "P")
            return UnprocessableEntity(new ApiResponse(false,
                $"La llave '{llave.Codigo}' está actualmente prestada y no puede reservarse."));

        // Verificar solapamiento de reservas confirmadas/pendientes
        bool solapamiento = await _db.Reservas.AnyAsync(r =>
            r.IdLlave == dto.IdLlave &&
            (r.Estado == "P" || r.Estado == "C") &&
            r.FechaInicio < dto.FechaFin &&
            r.FechaFin > dto.FechaInicio);

        if (solapamiento)
            return UnprocessableEntity(new ApiResponse(false,
                $"La llave '{llave.Codigo}' ya tiene una reserva en ese rango de fechas."));

        bool personaExiste = await _db.Personas.AnyAsync(p => p.IdPersona == dto.IdPersona);
        if (!personaExiste)
            return NotFound(new ApiResponse(false, $"Persona con Id={dto.IdPersona} no encontrada."));

        bool usuarioExiste = await _db.Usuarios.AnyAsync(u => u.IdUsuario == dto.IdUsuario);
        if (!usuarioExiste)
            return NotFound(new ApiResponse(false, $"Usuario con Id={dto.IdUsuario} no encontrado."));

        var reserva = new Reserva
        {
            IdLlave    = dto.IdLlave,
            IdPersona  = dto.IdPersona,
            IdUsuario  = dto.IdUsuario,
            FechaInicio = dto.FechaInicio,
            FechaFin   = dto.FechaFin,
            Estado     = "P"
        };

        _db.Reservas.Add(reserva);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Reserva creada: Id={Id}, Llave={Llave}, Persona={Persona}",
            reserva.IdReserva, llave.Codigo, dto.IdPersona);

        return Created($"/api/reservas/{reserva.IdReserva}",
            new ApiResponse(true, "Reserva registrada exitosamente.",
                new { reserva.IdReserva, reserva.Estado }));
    }

    // ── PATCH /api/reservas/{id}/cancelar ────────────────────
    [HttpPatch("{id:int}/cancelar")]
    public async Task<IActionResult> Cancelar(int id)
    {
        var reserva = await _db.Reservas.FindAsync(id);
        if (reserva == null)
            return NotFound(new ApiResponse(false, $"Reserva con Id={id} no encontrada."));

        if (reserva.Estado == "U")
            return UnprocessableEntity(new ApiResponse(false, "No se puede cancelar una reserva ya utilizada."));

        if (reserva.Estado == "X")
            return UnprocessableEntity(new ApiResponse(false, "La reserva ya fue cancelada."));

        reserva.Estado = "X";
        await _db.SaveChangesAsync();

        _logger.LogInformation("Reserva cancelada: Id={Id}", id);
        return Ok(new ApiResponse(true, "Reserva cancelada exitosamente.", new { reserva.IdReserva, reserva.Estado }));
    }
}
