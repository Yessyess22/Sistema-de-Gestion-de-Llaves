using System.ComponentModel.DataAnnotations;

namespace SistemaGestionLlaves.Models;

/// <summary>
/// Rol de usuario (Administrador, Operador, Consultor)
/// </summary>
public class Rol
{
    public int IdRol { get; set; }

    [Required, MaxLength(80)]
    public string NombreRol { get; set; } = string.Empty;

    [MaxLength(250)]
    public string? Descripcion { get; set; }

    /// <summary>Estado: A=Activo, I=Inactivo</summary>
    [Required, MaxLength(1)]
    public string Estado { get; set; } = "A";

    // Navegación
    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    public ICollection<RolPermiso> RolPermisos { get; set; } = new List<RolPermiso>();
}
