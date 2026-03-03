using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestionLlaves.Data;
using SistemaGestionLlaves.Models;

namespace SistemaGestionLlaves.Controllers
{
    [Route("api/reservas")]
    [ApiController]
    public class ApiReservasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ApiReservasController(ApplicationDbContext context)
        {
            _context = context;
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
                .ToListAsync();

            return Ok(reservas);
        }

        // 🔹 CREAR RESERVA
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] Reserva model)
        {
            if (model.FechaFin <= model.FechaInicio)
                return BadRequest(new { exito = false, mensaje = "La fecha fin debe ser mayor a la fecha inicio." });

            // 🔥 VALIDACIÓN PROFESIONAL: NO SOLAPAMIENTO
            bool existeConflicto = await _context.Reservas.AnyAsync(r =>
                r.IdLlave == model.IdLlave &&
                r.Estado != "X" && // no canceladas
                model.FechaInicio < r.FechaFin &&
                model.FechaFin > r.FechaInicio
            );

            if (existeConflicto)
                return BadRequest(new { exito = false, mensaje = "La llave ya está reservada en ese rango de fechas." });

            model.Estado = "P";

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