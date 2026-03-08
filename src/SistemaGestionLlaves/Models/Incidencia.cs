using System.ComponentModel.DataAnnotations;

namespace SistemaGestionLlaves.Models
{
    /// <summary>
    /// Registro de problemas o mantenimiento de una llave o cerradura.
    /// </summary>
    public class Incidencia
    {
        [Key]
        public int IdIncidencia { get; set; }

        [Required]
        public int IdLlave { get; set; }

        [Required(ErrorMessage = "El tipo de incidencia es obligatorio")]
        [MaxLength(50)]
        public string TipoIncidencia { get; set; } = string.Empty; // "Llave Dañada", "Cerradura Dañada", "Extravío", "Mantenimiento Preventivo"

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [MaxLength(500)]
        public string Descripcion { get; set; } = string.Empty;

        [Required]
        public DateTime FechaReporte { get; set; } = DateTime.UtcNow;

        public DateTime? FechaResolucion { get; set; }

        /// <summary>Estado: A=Abierta (Bloquea la llave), R=Resuelta</summary>
        [Required]
        [MaxLength(1)]
        [RegularExpression("^[AR]$")]
        public string Estado { get; set; } = "A";

        [MaxLength(200)]
        public string? NotasResolucion { get; set; }

        // Navegación
        public Llave Llave { get; set; } = null!;
    }
}
