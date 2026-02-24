using System.ComponentModel.DataAnnotations;

namespace SistemaGestionLlaves.Models;

/// <summary>
/// Clasifica los ambientes: Oficina, Laboratorio, Depósito, etc.
/// </summary>
public class TipoAmbiente
{
    public int IdTipo { get; set; }

    [Required, MaxLength(80)]
    public string NombreTipo { get; set; } = string.Empty;

    // Navegación
    public ICollection<Ambiente> Ambientes { get; set; } = new List<Ambiente>();
}
