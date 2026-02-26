using Ambientes.Data.Models;
using Ambientes.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace Ambientes.API.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AmbientesController : ControllerBase
    {
        private readonly IAmbienteService _service;
        private readonly ILogger<AmbientesController> _logger;

        public AmbientesController(IAmbienteService service, ILogger<AmbientesController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Ambiente>>> ObtenerTodos()
        {
            try
            {
                _logger.LogInformation("Obteniendo todos los ambientes");
                var ambientes = await _service.ObtenerTodos();
                return Ok(ambientes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ambientes");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { mensaje = "Error al obtener los ambientes", detalle = ex.Message });
            }
        }

       
        [HttpGet("{id}")]
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
                var ambiente = await _service.ObtenerPorId(id);
                return Ok(ambiente);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Ambiente no encontrado con ID: {id}", id);
                return NotFound(new { mensaje = ex.Message });
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
                _logger.LogInformation("Creando nuevo ambiente con código: {codigo}", ambiente.Codigo);
                var nuevoAmbiente = await _service.Crear(ambiente);
                return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevoAmbiente.Id }, nuevoAmbiente);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validación fallida al crear ambiente");
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Ya existe"))
            {
                _logger.LogWarning(ex, "Intento de crear ambiente duplicado");
                return Conflict(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear ambiente");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { mensaje = "Error al crear el ambiente", detalle = ex.Message });
            }
        }

       
        [HttpPut("{id}")]
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

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Intento de actualizar ambiente con datos inválidos");
                return BadRequest(ModelState);
            }

            try
            {
                _logger.LogInformation("Actualizando ambiente con ID: {id}", id);
                var ambienteActualizado = await _service.Actualizar(id, ambiente);
                return Ok(ambienteActualizado);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Ambiente no encontrado para actualizar con ID: {id}", id);
                return NotFound(new { mensaje = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validación fallida al actualizar ambiente");
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Ya existe"))
            {
                _logger.LogWarning(ex, "Intento de actualizar con código duplicado");
                return Conflict(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar ambiente");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { mensaje = "Error al actualizar el ambiente", detalle = ex.Message });
            }
        }

        [HttpDelete("{id}")]
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
                var resultado = await _service.Eliminar(id);
                
                if (!resultado)
                {
                    _logger.LogWarning("Ambiente no encontrado para eliminar con ID: {id}", id);
                    return NotFound(new { mensaje = $"No se encontró ambiente con ID {id}" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar ambiente");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { mensaje = "Error al eliminar el ambiente", detalle = ex.Message });
            }
        }
    }
}
