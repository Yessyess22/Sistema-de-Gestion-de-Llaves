using System.ComponentModel.DataAnnotations;

namespace SistemaGestionLlaves.Models;

/// <summary>
/// Ambiente físico (aula, laboratorio, oficina, etc.)
/// </summary>
public class Ambiente
{
    public int IdAmbiente { get; set; }

    [Required, MaxLength(20)]
    public string Codigo { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Ubicacion { get; set; }

    public int IdTipo { get; set; }

    /// <summary>Estado: A=Activo, I=Inactivo</summary>
    [Required, MaxLength(1)]
    public string Estado { get; set; } = "A";

    // Navegación
    public TipoAmbiente TipoAmbiente { get; set; } = null!;
    public ICollection<Llave> Llaves { get; set; } = new List<Llave>();
}
