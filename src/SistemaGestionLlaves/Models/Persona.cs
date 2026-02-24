using System.ComponentModel.DataAnnotations;

namespace SistemaGestionLlaves.Models;

/// <summary>
/// Representa una persona registrada en el sistema (docente, estudiante, administrativo, etc.)
/// </summary>
public class Persona
{
    public int IdPersona { get; set; }

    [Required, MaxLength(100)]
    public string Nombres { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Apellidos { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Ci { get; set; } = string.Empty;

    public DateOnly? FechaNacimiento { get; set; }

    [MaxLength(1)]
    public string? Genero { get; set; } // M, F, O

    [MaxLength(150)]
    public string? Correo { get; set; }

    [MaxLength(20)]
    public string? Celular { get; set; }

    /// <summary>Estado: A=Activo, I=Inactivo</summary>
    [Required, MaxLength(1)]
    public string Estado { get; set; } = "A";

    // Navegación
    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    public ICollection<PersonaAutorizada> PersonasAutorizadas { get; set; } = new List<PersonaAutorizada>();
    public ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
