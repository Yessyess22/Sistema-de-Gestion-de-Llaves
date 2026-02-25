using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ambientes.Data.Models
{
    /// <summary>
    /// Modelo que representa un Ambiente en el sistema.
    /// </summary>
    [Table("ambientes")]
    public class Ambiente
    {
        /// <summary>
        /// Identificador único del ambiente.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// Código único del ambiente (máximo 50 caracteres).
        /// </summary>
        [Required(ErrorMessage = "El código del ambiente es requerido")]
        [StringLength(50, ErrorMessage = "El código no puede exceder 50 caracteres")]
        [Column("codigo")]
        public string Codigo { get; set; } = string.Empty;

        /// <summary>
        /// Nombre descriptivo del ambiente (máximo 100 caracteres).
        /// </summary>
        [Required(ErrorMessage = "El nombre del ambiente es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de ambiente (ej: Clase, Laboratorio, Sala de Conferencias).
        /// </summary>
        [Required(ErrorMessage = "El tipo de ambiente es requerido")]
        [StringLength(50)]
        [Column("tipo_ambiente")]
        public string TipoAmbiente { get; set; } = string.Empty;

        /// <summary>
        /// Ubicación del ambiente (edificio, piso, etc).
        /// </summary>
        [Required(ErrorMessage = "La ubicación del ambiente es requerida")]
        [StringLength(100)]
        [Column("ubicacion")]
        public string Ubicacion { get; set; } = string.Empty;

        /// <summary>
        /// Estado del ambiente (Disponible, Ocupado, Mantenimiento).
        /// </summary>
        [Required(ErrorMessage = "El estado del ambiente es requerido")]
        [StringLength(30)]
        [Column("estado")]
        public string Estado { get; set; } = string.Empty;

        /// <summary>
        /// Fecha de creación del registro.
        /// </summary>
        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha de última actualización del registro.
        /// </summary>
        [Column("fecha_actualizacion")]
        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;
    }
}
