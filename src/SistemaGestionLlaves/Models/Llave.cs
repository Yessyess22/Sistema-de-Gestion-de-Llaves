using System.ComponentModel.DataAnnotations;

namespace SistemaGestionLlaves.Models;

/// <summary>
/// Llave física de un ambiente.
/// </summary>
public class Llave
{
    public int IdLlave { get; set; }

    [Required(ErrorMessage = "El código es obligatorio")]
    [MaxLength(30)]
    public string Codigo { get; set; } = string.Empty;

    /// <summary>Número de copias físicas que existen</summary>
    [Range(0, 100, ErrorMessage = "El número de copias debe estar entre 0 y 100")]
    public int NumCopias { get; set; } = 1;

    public int IdAmbiente { get; set; }

    /// <summary>Indica si es llave maestra (da acceso a múltiples ambientes)</summary>
    public bool EsMaestra { get; set; } = false;

    /// <summary>Estado: D=Disponible, P=Prestada, R=Reservada, I=Inactiva, M=Mantenimiento</summary>
    [Required]
    [MaxLength(1)]
    [RegularExpression("^[DPRIM]$")]
    public string Estado { get; set; } = "D";

    [MaxLength(300)]
    public string? Observaciones { get; set; }

    // Navegación
    public Ambiente? Ambiente { get; set; }
    public ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
    public ICollection<Incidencia> Incidencias { get; set; } = new List<Incidencia>();
}
