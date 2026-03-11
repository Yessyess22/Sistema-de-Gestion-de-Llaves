using System.ComponentModel.DataAnnotations;

namespace SistemaGestionLlaves.Models;

/// <summary>
/// Cuenta de acceso al sistema. Vinculada a una Persona y un Rol.
/// </summary>
public class Usuario
{
    public int IdUsuario { get; set; }

    public int IdPersona { get; set; }
    public int IdRol { get; set; }

    [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
    [StringLength(80, MinimumLength = 4, ErrorMessage = "El nombre de usuario debe tener entre 4 y 80 caracteres")]
    [RegularExpression(@"^[a-zA-Z0-9ñÑáéíóúÁÉÍÓÚ]*$", ErrorMessage = "El nombre de usuario no puede contener caracteres especiales ni espacios.")]
    [Display(Name = "Nombre de Usuario")]
    public string NombreUsuario { get; set; } = string.Empty;

    /// <summary>Hash BCrypt de la contraseña. NUNCA texto plano.</summary>
    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [MaxLength(200)]
    public string PasswordHash { get; set; } = string.Empty;

    [DataType(DataType.DateTime)]
    [Display(Name = "Fecha de Inicio")]
    public DateTime? FechaInicio { get; set; }

    [DataType(DataType.DateTime)]
    [Display(Name = "Fecha de Fin")]
    public DateTime? FechaFin { get; set; }

    /// <summary>Estado: A=Activo, I=Inactivo, B=Bloqueado</summary>
    [Required]
    [MaxLength(1)]
    [RegularExpression("^[AIB]$")]
    public string Estado { get; set; } = "A";

    // Navegación
    [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    public Persona Persona { get; set; } = null!;
    [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    public Rol Rol { get; set; } = null!;
    public ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
