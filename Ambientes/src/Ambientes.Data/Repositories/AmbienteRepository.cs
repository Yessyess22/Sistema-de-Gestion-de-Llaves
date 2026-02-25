using Ambientes.Data.Context;
using Ambientes.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Ambientes.Data.Repositories
{
    /// <summary>
    /// Repositorio para acceso a datos de Ambientes.
    /// Encapsula operaciones CRUD contra la base de datos.
    /// </summary>
    public class AmbienteRepository
    {
        private readonly AmbientesDbContext _context;

        public AmbienteRepository(AmbientesDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Obtiene todos los ambientes.
        /// </summary>
        public async Task<List<Ambiente>> ObtenerTodos()
        {
            return await _context.Ambientes
                .OrderBy(a => a.Nombre)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene un ambiente por su ID.
        /// </summary>
        public async Task<Ambiente?> ObtenerPorId(int id)
        {
            if (id <= 0)
                throw new ArgumentException("El ID debe ser mayor a cero", nameof(id));

            return await _context.Ambientes
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        /// <summary>
        /// Obtiene un ambiente por su código.
        /// </summary>
        public async Task<Ambiente?> ObtenerPorCodigo(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                throw new ArgumentException("El código no puede estar vacío", nameof(codigo));

            return await _context.Ambientes
                .FirstOrDefaultAsync(a => a.Codigo == codigo.Trim());
        }

        /// <summary>
        /// Obtiene ambientes por estado.
        /// </summary>
        public async Task<List<Ambiente>> ObtenerPorEstado(string estado)
        {
            if (string.IsNullOrWhiteSpace(estado))
                throw new ArgumentException("El estado no puede estar vacío", nameof(estado));

            return await _context.Ambientes
                .Where(a => a.Estado == estado.Trim())
                .OrderBy(a => a.Nombre)
                .ToListAsync();
        }

        /// <summary>
        /// Crea un nuevo ambiente en la base de datos.
        /// </summary>
        public async Task<Ambiente> Crear(Ambiente ambiente)
        {
            if (ambiente == null)
                throw new ArgumentNullException(nameof(ambiente));

            // Validar que el código no exista
            var existente = await _context.Ambientes
                .FirstOrDefaultAsync(a => a.Codigo == ambiente.Codigo);
            
            if (existente != null)
                throw new InvalidOperationException($"Ya existe un ambiente con el código '{ambiente.Codigo}'");

            ambiente.FechaCreacion = DateTime.UtcNow;
            ambiente.FechaActualizacion = DateTime.UtcNow;

            _context.Ambientes.Add(ambiente);
            await _context.SaveChangesAsync();

            return ambiente;
        }

        /// <summary>
        /// Actualiza un ambiente existente.
        /// </summary>
        public async Task<Ambiente> Actualizar(Ambiente ambiente)
        {
            if (ambiente == null)
                throw new ArgumentNullException(nameof(ambiente));

            if (ambiente.Id <= 0)
                throw new ArgumentException("El ID debe ser mayor a cero", nameof(ambiente));

            var existente = await _context.Ambientes
                .FirstOrDefaultAsync(a => a.Id == ambiente.Id);

            if (existente == null)
                throw new KeyNotFoundException($"No se encontró ambiente con ID {ambiente.Id}");

            // Validar código único (si cambió)
            if (existente.Codigo != ambiente.Codigo)
            {
                var codigoExistente = await _context.Ambientes
                    .FirstOrDefaultAsync(a => a.Codigo == ambiente.Codigo);
                
                if (codigoExistente != null)
                    throw new InvalidOperationException($"Ya existe otro ambiente con el código '{ambiente.Codigo}'");
            }

            existente.Codigo = ambiente.Codigo;
            existente.Nombre = ambiente.Nombre;
            existente.TipoAmbiente = ambiente.TipoAmbiente;
            existente.Ubicacion = ambiente.Ubicacion;
            existente.Estado = ambiente.Estado;
            existente.FechaActualizacion = DateTime.UtcNow;

            _context.Ambientes.Update(existente);
            await _context.SaveChangesAsync();

            return existente;
        }

        /// <summary>
        /// Elimina un ambiente por su ID.
        /// </summary>
        public async Task<bool> Eliminar(int id)
        {
            if (id <= 0)
                throw new ArgumentException("El ID debe ser mayor a cero", nameof(id));

            var ambiente = await _context.Ambientes
                .FirstOrDefaultAsync(a => a.Id == id);

            if (ambiente == null)
                return false;

            _context.Ambientes.Remove(ambiente);
            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Verifica si existe un ambiente con el ID especificado.
        /// </summary>
        public async Task<bool> Existe(int id)
        {
            if (id <= 0)
                return false;

            return await _context.Ambientes
                .AnyAsync(a => a.Id == id);
        }
    }
}
