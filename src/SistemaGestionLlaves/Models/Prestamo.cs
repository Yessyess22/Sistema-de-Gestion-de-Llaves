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

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime FechaHoraPrestamo { get; set; } = DateTime.UtcNow;

    [DataType(DataType.DateTime)]
    public DateTime? FechaHoraDevolucionEsperada { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime? FechaHoraDevolucionReal { get; set; }

    /// <summary>Estado: A=Activo (prestado), D=Devuelto, V=Vencido, C=Cancelado</summary>
    [Required]
    [MaxLength(1)]
    [RegularExpression("^[ADVC]$")]
    public string Estado { get; set; } = "A";

    [MaxLength(300, ErrorMessage = "Las observaciones no pueden exceder los 300 caracteres")]
    public string? Observaciones { get; set; }

    /// <summary>Firma digital del solicitante en formato Base64 (imagen PNG).</summary>
    public string? FirmaBase64 { get; set; }

    // Navegación
    public Llave Llave { get; set; } = null!;
    public Persona Persona { get; set; } = null!;
    public Usuario Usuario { get; set; } = null!;
}
