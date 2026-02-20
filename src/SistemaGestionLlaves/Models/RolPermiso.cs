namespace SistemaGestionLlaves.Models;

/// <summary>
/// Tabla intermedia Rol - Permiso (muchos a muchos)
/// </summary>
public class RolPermiso
{
    public int IdRol { get; set; }
    public int IdPermiso { get; set; }

    // Navegación
    public Rol Rol { get; set; } = null!;
    public Permiso Permiso { get; set; } = null!;
}
