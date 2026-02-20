using System.ComponentModel.DataAnnotations;

namespace SistemaGestionLlaves.Models;

/// <summary>
/// Alertas y notificaciones del sistema (devoluciones vencidas, llaves perdidas, etc.)
/// </summary>
public class AlertaNotificacion
{
    public int IdAlerta { get; set; }

    /// <summary>VENCIMIENTO, PERDIDA, DEVOLUCION, RESERVA</summary>
    [Required, MaxLength(50)]
    public string TipoAlerta { get; set; } = string.Empty;

    /// <summary>FK nullable al préstamo relacionado</summary>
    public int? IdPrestamo { get; set; }

    /// <summary>FK nullable a la llave relacionada</summary>
    public int? IdLlave { get; set; }

    [Required, MaxLength(500)]
    public string Mensaje { get; set; } = string.Empty;

    public DateTime FechaGenerada { get; set; } = DateTime.UtcNow;

    public bool Leida { get; set; } = false;

    // Navegación
    public Prestamo? Prestamo { get; set; }
    public Llave? Llave { get; set; }
}
