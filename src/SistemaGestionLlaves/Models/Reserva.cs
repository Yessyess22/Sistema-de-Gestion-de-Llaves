using System.ComponentModel.DataAnnotations;

namespace SistemaGestionLlaves.Models;

/// <summary>
/// Reserva anticipada de una llave para un rango de fechas.
/// </summary>
public class Reserva
{
    public int IdReserva { get; set; }

    public int IdLlave { get; set; }
    public int IdPersona { get; set; }
    public int IdUsuario { get; set; }   // Operador que registró la reserva

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime FechaInicio { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime FechaFin { get; set; }

    /// <summary>Estado: P=Pendiente, C=Confirmada, U=Utilizada, X=Cancelada</summary>
    [Required]
    [MaxLength(1)]
    [RegularExpression("^[PCUX]$")]
    public string Estado { get; set; } = "P";

    // Navegación
    public Llave Llave { get; set; } = null!;
    public Persona Persona { get; set; } = null!;
    public Usuario Usuario { get; set; } = null!;
}
