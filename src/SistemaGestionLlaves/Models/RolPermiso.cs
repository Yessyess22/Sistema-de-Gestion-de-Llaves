using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaGestionLlaves.Models;

/// <summary>
/// Tabla intermedia que vincula Roles con Permisos (relación muchos a muchos).
/// Basado en tabla: RolPermisos
/// </summary>
[Table("RolPermisos")]
public class RolPermiso
{
    /// <summary>Identificador del rol (clave foránea). (id_rol)</summary>
    [Required]
    public int IdRol { get; set; }

    /// <summary>Identificador del permiso (clave foránea). (id_permiso)</summary>
    [Required]
    public int IdPermiso { get; set; }

    /// <summary>Navegación: Referencia al Rol</summary>
    [ForeignKey(nameof(IdRol))]
    public virtual Rol Rol { get; set; } = null!;

    /// <summary>Navegación: Referencia al Permiso</summary>
    [ForeignKey(nameof(IdPermiso))]
    public virtual Permiso Permiso { get; set; } = null!;
}
