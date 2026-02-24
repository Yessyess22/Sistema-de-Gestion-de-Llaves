using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaGestionLlaves.Models;

/// <summary>
/// Rol de usuario del sistema (Administrador, Operador, Consultor, etc.).
/// Basado en tabla: Rol
/// </summary>
[Table("Rol")]
public class Rol
{
    /// <summary>Identificador único del rol. (id_rol)</summary>
    [Key]
    public int IdRol { get; set; }

    /// <summary>Nombre único del rol (máximo 80 caracteres). (nombre_rol)</summary>
    [Required(ErrorMessage = "El nombre del rol es requerido")]
    [StringLength(80, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 80 caracteres")]
    public string NombreRol { get; set; } = string.Empty;

    /// <summary>Descripción del rol (máximo 250 caracteres). (descripcion)</summary>
    [StringLength(250, ErrorMessage = "La descripción no puede exceder 250 caracteres")]
    public string? Descripcion { get; set; }

    /// <summary>
    /// Estado del rol: A=Activo, I=Inactivo.
    /// Por defecto: A (Activo)
    /// (estado)
    /// </summary>
    [Required(ErrorMessage = "El estado es requerido")]
    [StringLength(1)]
    [RegularExpression(@"^[AI]$", ErrorMessage = "El estado debe ser 'A' (Activo) o 'I' (Inactivo)")]
    public string Estado { get; set; } = "A";

    /// <summary>Navegación: Usuarios con este rol</summary>
    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();

    /// <summary>Navegación: Permisos asignados a este rol</summary>
    public virtual ICollection<RolPermiso> RolPermisos { get; set; } = new List<RolPermiso>();
}
