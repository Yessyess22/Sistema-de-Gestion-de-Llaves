using System.ComponentModel.DataAnnotations;

namespace SistemaGestionLlaves.Models;

/// <summary>
/// Registro de intentos de inicio de sesión (exitosos y fallidos).
/// </summary>
public class IntentoAcceso
{
    public int IdIntento { get; set; }

    [Required, MaxLength(80)]
    public string NombreUsuario { get; set; } = string.Empty;

    public DateTime FechaHora { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string? Ip { get; set; }

    public bool Exitoso { get; set; } = false;
}
