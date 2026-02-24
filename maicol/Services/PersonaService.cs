using Microsoft.EntityFrameworkCore;
using SistemaWeb.Data;
using SistemaWeb.Models;

namespace SistemaWeb.Services
{
    /// <summary>
    /// Interfaz para el servicio de Personas.
    /// Define las operaciones CRUD y otras operaciones de negocio.
    /// </summary>
    public interface IPersonaService
    {
        Task<IEnumerable<Persona>> ObtenerTodasAsync();
        Task<Persona?> ObtenerPorIdAsync(int id);
        Task<Persona> CrearAsync(Persona persona);
        Task<Persona> ActualizarAsync(Persona persona);
        Task<bool> EliminarAsync(int id);
        Task<bool> ExistePorEmailAsync(string email, int? idExcluir = null);
        Task<bool> ExistePorCIAsync(string ci, int? idExcluir = null);
        Task<Persona?> ObtenerPorEmailAsync(string email);
        Task<IEnumerable<Persona>> BuscarPorNombreAsync(string nombre);
        Task<IEnumerable<Persona>> ObtenerPorTipoAsync(string tipo);
    }

    /// <summary>
    /// Servicio de Personas - Implementa la lógica de negocio.
    /// Maneja todas las operaciones CRUD y validaciones de datos.
    /// </summary>
    public class PersonaService : IPersonaService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PersonaService> _logger;

        public PersonaService(ApplicationDbContext context, ILogger<PersonaService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todas las personas de la base de datos.
        /// </summary>
        public async Task<IEnumerable<Persona>> ObtenerTodasAsync()
        {
            try
            {
                _logger.LogInformation("Obteniendo todas las personas");
                return await _context.Personas
                    .OrderBy(p => p.Nombres)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las personas");
                throw;
            }
        }

        /// <summary>
        /// Obtiene una persona por su ID.
        /// </summary>
        public async Task<Persona?> ObtenerPorIdAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Obteniendo persona con ID: {id}");
                return await _context.Personas.FirstOrDefaultAsync(p => p.IdPersona == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener persona con ID: {id}");
                throw;
            }
        }

        /// <summary>
        /// Crea una nueva persona.
        /// </summary>
        public async Task<Persona> CrearAsync(Persona persona)
        {
            try
            {
                // Validar que el email no exista
                if (await ExistePorEmailAsync(persona.Email))
                {
                    throw new InvalidOperationException($"Ya existe una persona con el correo: {persona.Email}");
                }

                // Validar que el CI no exista
                if (await ExistePorCIAsync(persona.CI))
                {
                    throw new InvalidOperationException($"Ya existe una persona con el CI: {persona.CI}");
                }

                _logger.LogInformation($"Creando nueva persona: {persona.NombreCompleto}");

                _context.Personas.Add(persona);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Persona creada exitosamente con ID: {persona.IdPersona}");
                return persona;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error de base de datos al crear persona");
                throw new InvalidOperationException("Error al crear la persona. Verifique los datos ingresados.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al crear persona");
                throw;
            }
        }

        /// <summary>
        /// Actualiza una persona existente.
        /// </summary>
        public async Task<Persona> ActualizarAsync(Persona persona)
        {
            try
            {
                var personaExistente = await ObtenerPorIdAsync(persona.IdPersona);
                if (personaExistente == null)
                {
                    throw new KeyNotFoundException($"No se encontró la persona con ID: {persona.IdPersona}");
                }

                // Validar email si cambió
                if (personaExistente.Email != persona.Email && await ExistePorEmailAsync(persona.Email, persona.IdPersona))
                {
                    throw new InvalidOperationException($"Ya existe otra persona con el correo: {persona.Email}");
                }

                // Validar CI si cambió
                if (personaExistente.CI != persona.CI && await ExistePorCIAsync(persona.CI, persona.IdPersona))
                {
                    throw new InvalidOperationException($"Ya existe otra persona con el CI: {persona.CI}");
                }

                _logger.LogInformation($"Actualizando persona con ID: {persona.IdPersona}");

                _context.Personas.Update(persona);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Persona actualizada exitosamente: {persona.NombreCompleto}");
                return persona;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, $"Error de base de datos al actualizar persona {persona.IdPersona}");
                throw new InvalidOperationException("Error al actualizar la persona.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error inesperado al actualizar persona {persona.IdPersona}");
                throw;
            }
        }

        /// <summary>
        /// Elimina una persona.
        /// </summary>
        public async Task<bool> EliminarAsync(int id)
        {
            try
            {
                var persona = await ObtenerPorIdAsync(id);
                if (persona == null)
                {
                    throw new KeyNotFoundException($"No se encontró la persona con ID: {id}");
                }

                _logger.LogInformation($"Eliminando persona con ID: {id}");

                _context.Personas.Remove(persona);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Persona eliminada exitosamente: {persona.NombreCompleto}");
                return true;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, $"Error de base de datos al eliminar persona {id}");
                throw new InvalidOperationException("Error al eliminar la persona.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error inesperado al eliminar persona {id}");
                throw;
            }
        }

        /// <summary>
        /// Verifica si existe una persona con un correo específico.
        /// </summary>
        public async Task<bool> ExistePorEmailAsync(string email, int? idExcluir = null)
        {
            try
            {
                var query = _context.Personas.Where(p => p.Email == email);

                if (idExcluir.HasValue)
                {
                    query = query.Where(p => p.IdPersona != idExcluir.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al verificar existencia de email: {email}");
                throw;
            }
        }

        /// <summary>
        /// Verifica si existe una persona con un CI específico.
        /// </summary>
        public async Task<bool> ExistePorCIAsync(string ci, int? idExcluir = null)
        {
            try
            {
                var query = _context.Personas.Where(p => p.CI == ci);

                if (idExcluir.HasValue)
                {
                    query = query.Where(p => p.IdPersona != idExcluir.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al verificar existencia de CI: {ci}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene una persona por su correo electrónico.
        /// </summary>
        public async Task<Persona?> ObtenerPorEmailAsync(string email)
        {
            try
            {
                return await _context.Personas
                    .FirstOrDefaultAsync(p => p.Email == email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener persona por email: {email}");
                throw;
            }
        }

        /// <summary>
        /// Busca personas por nombre (nombres o apellidos).
        /// </summary>
        public async Task<IEnumerable<Persona>> BuscarPorNombreAsync(string nombre)
        {
            try
            {
                _logger.LogInformation($"Buscando personas por nombre: {nombre}");

                var nombreBusqueda = nombre.ToLower();
                return await _context.Personas
                    .Where(p => p.Nombres.ToLower().Contains(nombreBusqueda) ||
                           p.ApellidoPaterno.ToLower().Contains(nombreBusqueda) ||
                           p.ApellidoMaterno.ToLower().Contains(nombreBusqueda))
                    .OrderBy(p => p.Nombres)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al buscar personas por nombre: {nombre}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene personas por tipo ("Documento" o "Empresa").
        /// </summary>
        public async Task<IEnumerable<Persona>> ObtenerPorTipoAsync(string tipo)
        {
            try
            {
                _logger.LogInformation($"Obteniendo personas por tipo: {tipo}");

                return await _context.Personas
                    .Where(p => p.Tipo == tipo)
                    .OrderBy(p => p.Nombres)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener personas por tipo: {tipo}");
                throw;
            }
        }
    }
}
