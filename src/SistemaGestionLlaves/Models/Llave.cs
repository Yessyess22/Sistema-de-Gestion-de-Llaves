using System.ComponentModel.DataAnnotations;

namespace SistemaGestionLlaves.Models;

/// <summary>
/// Llave física de un ambiente.
/// </summary>
public class Llave
{
    public int IdLlave { get; set; }

    [Required, MaxLength(30)]
    public string Codigo { get; set; } = string.Empty;

    /// <summary>Número de copias físicas que existen</summary>
    public int NumCopias { get; set; } = 1;

    public int IdAmbiente { get; set; }

    /// <summary>Indica si es llave maestra (da acceso a múltiples ambientes)</summary>
    public bool EsMaestra { get; set; } = false;

    /// <summary>Estado: D=Disponible, P=Prestada, R=Reservada, I=Inactiva</summary>
    [Required, MaxLength(1)]
    public string Estado { get; set; } = "D";

    [MaxLength(300)]
    public string? Observaciones { get; set; }

    // Navegación
    public Ambiente? Ambiente { get; set; }
    public ICollection<PersonaAutorizada> PersonasAutorizadas { get; set; } = new List<PersonaAutorizada>();
    public ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
    public ICollection<AlertaNotificacion> Alertas { get; set; } = new List<AlertaNotificacion>();
}
