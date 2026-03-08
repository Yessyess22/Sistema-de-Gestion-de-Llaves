using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestionLlaves.Data;
using SistemaGestionLlaves.Models;

namespace SistemaGestionLlaves.Controllers
{
    [Route("api/reservas")]
    [ApiController]
    public class ReservasApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReservasApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // DTO para recibir solo los campos que envía el formulario
        public class ReservaDto
        {
            public int IdLlave { get; set; }
            public int IdPersona { get; set; }
            public int IdUsuario { get; set; }
            public DateTime FechaInicio { get; set; }
            public DateTime FechaFin { get; set; }
        }

        // 🔹 LISTAR
        [HttpGet]
        public async Task<IActionResult> Get(string? estado)
        {
            var query = _context.Reservas
                .Include(r => r.Llave)
                .Include(r => r.Persona)
                .AsQueryable();

            if (!string.IsNullOrEmpty(estado))
                query = query.Where(r => r.Estado == estado);

            var reservas = await query
                .OrderByDescending(r => r.IdReserva)
                .Select(r => new
                {
                    r.IdReserva,
                    r.IdLlave,
                    r.IdPersona,
                    r.IdUsuario,
                    r.FechaInicio,
                    r.FechaFin,
                    r.Estado,
                    Llave = r.Llave != null ? r.Llave.Codigo : null,
                    Persona = r.Persona != null ? r.Persona.NombreCompleto : null
                })
                .ToListAsync();

            return Ok(reservas);
        }

        // 🔹 CREAR RESERVA
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] ReservaDto dto)
        {
            if (dto.IdLlave == 0 || dto.IdPersona == 0)
                return BadRequest(new { exito = false, mensaje = "Seleccione una llave y una persona." });

            // Normalizar fechas a UTC antes de cualquier operación con la BD
            var fechaInicio = DateTime.SpecifyKind(dto.FechaInicio, DateTimeKind.Utc);
            var fechaFin    = DateTime.SpecifyKind(dto.FechaFin,    DateTimeKind.Utc);

            if (fechaFin <= fechaInicio)
                return BadRequest(new { exito = false, mensaje = "La fecha fin debe ser mayor a la fecha inicio." });

            if (fechaInicio < DateTime.UtcNow.AddMinutes(-5)) // Pequeño margen para latencia
                return BadRequest(new { exito = false, mensaje = "La fecha de inicio no puede ser en el pasado." });

            // Verificar que la llave existe y no está en mantenimiento
            var llave = await _context.Llaves.FindAsync(dto.IdLlave);
            if (llave == null)
                return NotFound(new { exito = false, mensaje = "La llave no existe." });

            if (llave.Estado == "M")
                return BadRequest(new { exito = false, mensaje = $"La llave '{llave.Codigo}' está bloqueada por mantenimiento." });

            // VALIDACIÓN: NO SOLAPAMIENTO
            bool existeConflicto = await _context.Reservas.AnyAsync(r =>
                r.IdLlave == dto.IdLlave &&
                (r.Estado == "P" || r.Estado == "C") &&
                fechaInicio < r.FechaFin &&
                fechaFin > r.FechaInicio
            );

            if (existeConflicto)
                return BadRequest(new { exito = false, mensaje = "La llave ya está reservada en ese rango de fechas." });

            var model = new Reserva
            {
                IdLlave = dto.IdLlave,
                IdPersona = dto.IdPersona,
                IdUsuario = dto.IdUsuario > 0 ? dto.IdUsuario : 1,
                FechaInicio = fechaInicio,
                FechaFin    = fechaFin,
                Estado = "P"
            };

            _context.Reservas.Add(model);
            await _context.SaveChangesAsync();

            return Ok(new { exito = true, mensaje = "Reserva creada correctamente." });
        }

        // 🔹 CONFIRMAR
        [HttpPatch("{id}/confirmar")]
        public async Task<IActionResult> Confirmar(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva == null)
                return NotFound();

            if (reserva.Estado != "P")
                return BadRequest(new { exito = false, mensaje = "Solo reservas pendientes pueden confirmarse." });

            reserva.Estado = "C";
            await _context.SaveChangesAsync();

            return Ok(new { exito = true, mensaje = "Reserva confirmada." });
        }

        // 🔹 CANCELAR
        [HttpPatch("{id}/cancelar")]
        public async Task<IActionResult> Cancelar(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva == null)
                return NotFound();

            reserva.Estado = "X";
            await _context.SaveChangesAsync();

            return Ok(new { exito = true, mensaje = "Reserva cancelada." });
        }
    }
}