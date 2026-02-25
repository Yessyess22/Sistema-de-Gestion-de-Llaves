using Ambientes.Data.Models;

namespace Ambientes.Services.Interfaces
{
    /// <summary>
    /// Interfaz de servicio para operaciones relacionadas con Ambientes.
    /// Define el contrato que implementa la lógica de negocio.
    /// </summary>
    public interface IAmbienteService
    {
        /// <summary>
        /// Obtiene todos los ambientes.
        /// </summary>
        Task<IEnumerable<Ambiente>> ObtenerTodos();

        /// <summary>
        /// Obtiene un ambiente por su ID.
        /// </summary>
        Task<Ambiente?> ObtenerPorId(int id);

        /// <summary>
        /// Obtiene ambientes por estado.
        /// </summary>
        Task<IEnumerable<Ambiente>> ObtenerPorEstado(string estado);

        /// <summary>
        /// Crea un nuevo ambiente.
        /// </summary>
        Task<Ambiente> Crear(Ambiente ambiente);

        /// <summary>
        /// Actualiza un ambiente existente.
        /// </summary>
        Task<Ambiente> Actualizar(int id, Ambiente ambiente);

        /// <summary>
        /// Elimina un ambiente.
        /// </summary>
        Task<bool> Eliminar(int id);
    }
}
