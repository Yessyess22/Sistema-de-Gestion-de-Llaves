using System.ComponentModel.DataAnnotations;

namespace SistemaGestionLlaves.Models;

/// <summary>
/// Representa una persona registrada en el sistema (docente, estudiante, administrativo, etc.)
/// </summary>
public class Persona
{
    public int IdPersona { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MaxLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
    [RegularExpression(@"^[a-zA-Z0-9ñÑáéíóúÁÉÍÓÚ\s]*$", ErrorMessage = "No se permiten caracteres especiales.")]
    [Display(Name = "Nombres")]
    public string Nombres { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio")]
    [MaxLength(100, ErrorMessage = "El apellido no puede exceder los 100 caracteres")]
    [RegularExpression(@"^[a-zA-Z0-9ñÑáéíóúÁÉÍÓÚ\s]*$", ErrorMessage = "No se permiten caracteres especiales.")]
    [Display(Name = "Apellidos")]
    public string Apellidos { get; set; } = string.Empty;

    public string NombreCompleto => $"{Nombres} {Apellidos}";

    [Required(ErrorMessage = "La cédula de identidad (CI) es obligatoria")]
    [MaxLength(20, ErrorMessage = "El CI no puede exceder los 20 caracteres")]
    [RegularExpression(@"^[0-9]*$", ErrorMessage = "Solo se permiten números.")]
    [Display(Name = "Cédula de Identidad")]
    public string Ci { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Display(Name = "Fecha de Nacimiento")]
    public DateOnly? FechaNacimiento { get; set; }

    [MaxLength(1)]
    [RegularExpression("^[MFO]$|", ErrorMessage = "El género debe ser M, F o O")]
    [Display(Name = "Género")]
    public string? Genero { get; set; } // M, F, O

    [MaxLength(150, ErrorMessage = "El correo no puede exceder los 150 caracteres")]
    [EmailAddress(ErrorMessage = "El formato de correo electrónico no es válido")]
    [Display(Name = "Correo Electrónico")]
    public string? Correo { get; set; }

    [MaxLength(20, ErrorMessage = "El celular no puede exceder los 20 caracteres")]
    [Phone(ErrorMessage = "El formato de número telefónico no es válido")]
    [RegularExpression(@"^[0-9]*$", ErrorMessage = "Solo se permiten números.")]
    [Display(Name = "Celular/Teléfono")]
    public string? Celular { get; set; }

    /// <summary>Estado: A=Activo, I=Inactivo</summary>
    [Required]
    [MaxLength(1)]
    [RegularExpression("^[AI]$")]
    public string Estado { get; set; } = "A";

    // Navegación
    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    public ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
