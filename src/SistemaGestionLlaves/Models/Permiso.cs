using System.ComponentModel.DataAnnotations;

namespace SistemaGestionLlaves.Models;

/// <summary>
/// Permiso granular del sistema
/// </summary>
public class Permiso
{
    public int IdPermiso { get; set; }

    [Required, MaxLength(100)]
    public string NombrePermiso { get; set; } = string.Empty;

    [MaxLength(250)]
    public string? Descripcion { get; set; }

    // Navegación
    public ICollection<RolPermiso> RolPermisos { get; set; } = new List<RolPermiso>();
}
