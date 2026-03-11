using System.ComponentModel.DataAnnotations;

namespace SistemaGestionLlaves.Models;

/// <summary>
/// Ambiente físico (aula, laboratorio, oficina, etc.)
/// </summary>
public class Ambiente
{
    public int IdAmbiente { get; set; }

    [Required(ErrorMessage = "El código es obligatorio")]
    [MaxLength(20, ErrorMessage = "El código no puede exceder los 20 caracteres")]
    [RegularExpression(@"^[a-zA-Z0-9\s-]*$", ErrorMessage = "El código solo puede contener letras, números, espacios y guiones.")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre del ambiente es obligatorio")]
    [MaxLength(150, ErrorMessage = "El nombre no puede exceder los 150 caracteres")]
    [RegularExpression(@"^[a-zA-Z0-9ñÑáéíóúÁÉÍÓÚ\s]*$", ErrorMessage = "No se permiten caracteres especiales.")]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(200, ErrorMessage = "La ubicación no puede exceder los 200 caracteres")]
    [RegularExpression(@"^[a-zA-Z0-9ñÑáéíóúÁÉÍÓÚ\s,.-]*$", ErrorMessage = "La ubicación contiene caracteres no permitidos.")]
    public string? Ubicacion { get; set; }

    [Required(ErrorMessage = "El tipo de ambiente es obligatorio")]
    public int IdTipo { get; set; }

    /// <summary>Estado: A=Activo, I=Inactivo</summary>
    [Required]
    [MaxLength(1)]
    [RegularExpression("^[AI]$")]
    public string Estado { get; set; } = "A";

    // Navegación
    [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    public TipoAmbiente TipoAmbiente { get; set; } = null!;
    [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    public ICollection<Llave> Llaves { get; set; } = new List<Llave>();
}
