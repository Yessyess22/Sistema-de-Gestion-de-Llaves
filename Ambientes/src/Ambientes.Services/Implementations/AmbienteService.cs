using Ambientes.Data.Models;
using Ambientes.Data.Repositories;
using Ambientes.Services.Interfaces;

namespace Ambientes.Services.Implementations
{
    /// <summary>
    /// Servicio que implementa la lógica de negocio para Ambientes.
    /// Actúa como intermediario entre el controlador y el repositorio.
    /// </summary>
    public class AmbienteService : IAmbienteService
    {
        private readonly AmbienteRepository _repository;

        public AmbienteService(AmbienteRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <summary>
        /// Obtiene todos los ambientes ordenados por nombre.
        /// </summary>
        public async Task<IEnumerable<Ambiente>> ObtenerTodos()
        {
            try
            {
                return await _repository.ObtenerTodos();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error al obtener los ambientes", ex);
            }
        }

        /// <summary>
        /// Obtiene un ambiente por su ID.
        /// Lanza KeyNotFoundException si no existe.
        /// </summary>
        public async Task<Ambiente?> ObtenerPorId(int id)
        {
            if (id <= 0)
                throw new ArgumentException("El ID debe ser mayor a cero", nameof(id));

            try
            {
                var ambiente = await _repository.ObtenerPorId(id);
                
                if (ambiente == null)
                    throw new KeyNotFoundException($"No se encontró ambiente con ID {id}");

                return ambiente;
            }
            catch (Exception ex) when (ex is not KeyNotFoundException)
            {
                throw new InvalidOperationException("Error al obtener el ambiente", ex);
            }
        }

        /// <summary>
        /// Obtiene ambientes por estado.
        /// </summary>
        public async Task<IEnumerable<Ambiente>> ObtenerPorEstado(string estado)
        {
            if (string.IsNullOrWhiteSpace(estado))
                throw new ArgumentException("El estado no puede estar vacío", nameof(estado));

            try
            {
                return await _repository.ObtenerPorEstado(estado);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error al obtener ambientes por estado", ex);
            }
        }

        /// <summary>
        /// Crea un nuevo ambiente validando sus datos.
        /// </summary>
        public async Task<Ambiente> Crear(Ambiente ambiente)
        {
            if (ambiente == null)
                throw new ArgumentNullException(nameof(ambiente));

            ValidarAmbiente(ambiente);

            try
            {
                return await _repository.Crear(ambiente);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException(ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error al crear el ambiente", ex);
            }
        }

        /// <summary>
        /// Actualiza un ambiente existente validando sus datos.
        /// </summary>
        public async Task<Ambiente> Actualizar(int id, Ambiente ambiente)
        {
            if (id <= 0)
                throw new ArgumentException("El ID debe ser mayor a cero", nameof(id));

            if (ambiente == null)
                throw new ArgumentNullException(nameof(ambiente));

            ValidarAmbiente(ambiente);

            try
            {
                ambiente.Id = id;
                return await _repository.Actualizar(ambiente);
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(ex.Message, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException(ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error al actualizar el ambiente", ex);
            }
        }

        /// <summary>
        /// Elimina un ambiente por su ID.
        /// Retorna false si no existe el ambiente.
        /// </summary>
        public async Task<bool> Eliminar(int id)
        {
            if (id <= 0)
                throw new ArgumentException("El ID debe ser mayor a cero", nameof(id));

            try
            {
                return await _repository.Eliminar(id);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error al eliminar el ambiente", ex);
            }
        }

        /// <summary>
        /// Valida los datos de un ambiente.
        /// </summary>
        private void ValidarAmbiente(Ambiente ambiente)
        {
            if (string.IsNullOrWhiteSpace(ambiente.Codigo))
                throw new ArgumentException("El código es requerido", nameof(ambiente.Codigo));

            if (ambiente.Codigo.Length > 50)
                throw new ArgumentException("El código no puede exceder 50 caracteres", nameof(ambiente.Codigo));

            if (string.IsNullOrWhiteSpace(ambiente.Nombre))
                throw new ArgumentException("El nombre es requerido", nameof(ambiente.Nombre));

            if (ambiente.Nombre.Length > 100)
                throw new ArgumentException("El nombre no puede exceder 100 caracteres", nameof(ambiente.Nombre));

            if (string.IsNullOrWhiteSpace(ambiente.TipoAmbiente))
                throw new ArgumentException("El tipo de ambiente es requerido", nameof(ambiente.TipoAmbiente));

            if (string.IsNullOrWhiteSpace(ambiente.Ubicacion))
                throw new ArgumentException("La ubicación es requerida", nameof(ambiente.Ubicacion));

            if (string.IsNullOrWhiteSpace(ambiente.Estado))
                throw new ArgumentException("El estado es requerido", nameof(ambiente.Estado));

            // Validar que el estado sea uno de los permitidos
            var estadosValidos = new[] { "Disponible", "Ocupado", "Mantenimiento" };
            if (!estadosValidos.Contains(ambiente.Estado))
                throw new ArgumentException($"El estado debe ser uno de: {string.Join(", ", estadosValidos)}", nameof(ambiente.Estado));
        }
    }
}
