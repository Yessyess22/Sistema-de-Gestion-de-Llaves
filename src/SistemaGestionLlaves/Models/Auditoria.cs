using System.ComponentModel.DataAnnotations;

namespace SistemaGestionLlaves.Models;

/// <summary>
/// Registro de auditoría: toda operación importante queda trazada.
/// </summary>
public class Auditoria
{
    public int IdAuditoria { get; set; }

    [Required, MaxLength(100)]
    public string TablaAfectada { get; set; } = string.Empty;

    /// <summary>Operación: INSERT, UPDATE, DELETE, LOGIN, LOGOUT</summary>
    [Required, MaxLength(20)]
    public string Operacion { get; set; } = string.Empty;

    public int? IdRegistro { get; set; }

    public int? IdUsuario { get; set; }   // Nullable: operaciones del sistema sin usuario

    public DateTime FechaHora { get; set; } = DateTime.UtcNow;

    /// <summary>JSON con datos antes del cambio</summary>
    public string? DatosAnteriores { get; set; }

    /// <summary>JSON con datos después del cambio</summary>
    public string? DatosNuevos { get; set; }

    // Navegación
    public Usuario? Usuario { get; set; }
}
