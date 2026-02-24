using Microsoft.AspNetCore.Mvc;
using SistemaWeb.Models;
using SistemaWeb.Services;

namespace SistemaWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PersonaController : ControllerBase
    {
        private readonly IPersonaService _personaService;
        private readonly ILogger<PersonaController> _logger;

        public PersonaController(IPersonaService personaService, ILogger<PersonaController> logger)
        {
            _personaService = personaService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Persona>>> GetAll([FromQuery] string? busqueda = null)
        {
            try
            {
                IEnumerable<Persona> personas = string.IsNullOrEmpty(busqueda)
                    ? await _personaService.ObtenerTodasAsync()
                    : await _personaService.BuscarPorNombreAsync(busqueda);

                return Ok(personas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener lista de personas");
                return StatusCode(500, "Error al obtener personas.");
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Persona>> Get(int id)
        {
            try
            {
                var persona = await _personaService.ObtenerPorIdAsync(id);
                if (persona == null) return NotFound();
                return Ok(persona);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener persona {id}");
                return StatusCode(500, "Error al obtener la persona.");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Persona>> Create([FromBody] Persona persona)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                if (persona.Edad < 18)
                {
                    ModelState.AddModelError("FechaNac", "La persona debe ser mayor de 18 años");
                    return BadRequest(ModelState);
                }

                if (persona.Tipo != "Documento" && persona.Tipo != "Empresa")
                {
                    ModelState.AddModelError("Tipo", "El tipo debe ser 'Documento' o 'Empresa'");
                    return BadRequest(ModelState);
                }

                var creado = await _personaService.CrearAsync(persona);
                return CreatedAtAction(nameof(Get), new { id = creado.IdPersona }, creado);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación al crear persona");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear persona");
                return StatusCode(500, "Error al crear la persona.");
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Persona persona)
        {
            if (id != persona.IdPersona) return BadRequest("ID mismatch");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                if (persona.Edad < 18) return BadRequest(new { error = "La persona debe ser mayor de 18 años" });
                if (persona.Tipo != "Documento" && persona.Tipo != "Empresa") return BadRequest(new { error = "Tipo inválido" });

                var actualizado = await _personaService.ActualizarAsync(persona);
                return Ok(actualizado);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, $"Persona no encontrada: {id}");
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación al actualizar persona");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al actualizar persona {id}");
                return StatusCode(500, "Error al actualizar la persona.");
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _personaService.EliminarAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, $"Persona no encontrada: {id}");
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar persona {id}");
                return StatusCode(500, "Error al eliminar la persona.");
            }
        }

        [HttpGet("PorTipo")]
        public async Task<ActionResult<IEnumerable<Persona>>> PorTipo([FromQuery] string tipo)
        {
            if (string.IsNullOrEmpty(tipo)) return BadRequest(new { error = "Parametro 'tipo' requerido" });

            if (tipo != "Documento" && tipo != "Empresa") return BadRequest(new { error = "Tipo inválido" });

            try
            {
                var personas = await _personaService.ObtenerPorTipoAsync(tipo);
                return Ok(personas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener personas por tipo: {tipo}");
                return StatusCode(500, "Error al filtrar personas.");
            }
        }
    }
}
