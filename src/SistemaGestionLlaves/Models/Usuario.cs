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

    [Required, MaxLength(80)]
    public string NombreUsuario { get; set; } = string.Empty;

    /// <summary>Hash BCrypt de la contraseña. NUNCA texto plano.</summary>
    [Required, MaxLength(200)]
    public string PasswordHash { get; set; } = string.Empty;

    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }

    /// <summary>Estado: A=Activo, I=Inactivo, B=Bloqueado</summary>
    [Required, MaxLength(1)]
    public string Estado { get; set; } = "A";

    // Navegación
    public Persona Persona { get; set; } = null!;
    public Rol Rol { get; set; } = null!;
    public ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
    public ICollection<Auditoria> Auditorias { get; set; } = new List<Auditoria>();
}
