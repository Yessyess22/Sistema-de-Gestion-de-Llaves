using System.ComponentModel.DataAnnotations;

namespace SistemaGestionLlaves.Models;

/// <summary>
/// Registro de préstamo de una llave a una persona.
/// </summary>
public class Prestamo
{
    public int IdPrestamo { get; set; }

    public int IdLlave { get; set; }
    public int IdPersona { get; set; }
    public int IdUsuario { get; set; }   // Operador que realizó el préstamo

    public DateTime FechaHoraPrestamo { get; set; } = DateTime.UtcNow;
    public DateTime? FechaHoraDevolucionEsperada { get; set; }
    public DateTime? FechaHoraDevolucionReal { get; set; }

    /// <summary>Estado: A=Activo (prestado), D=Devuelto, V=Vencido, C=Cancelado</summary>
    [Required, MaxLength(1)]
    public string Estado { get; set; } = "A";

    [MaxLength(300)]
    public string? Observaciones { get; set; }

    // Navegación
    public Llave Llave { get; set; } = null!;
    public Persona Persona { get; set; } = null!;
    public Usuario Usuario { get; set; } = null!;
    public ICollection<AlertaNotificacion> Alertas { get; set; } = new List<AlertaNotificacion>();
}
