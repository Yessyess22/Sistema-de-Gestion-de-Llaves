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
    // ----------------------
    // Lógica de Usuario
    // ----------------------
    // Autenticación: verifica usuario y contraseña (MD5)
    public static bool Autenticar(SistemaGestionLlaves.Data.ApplicationDbContext db, string nombre, string password)
    {
        string hash = CalcularMD5(password);
        return db.Usuarios.Any(u => u.NombreUsuario == nombre && u.PasswordHash == hash);
    }

    // Calcula el hash MD5 de una cadena
    public static string CalcularMD5(string input)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(input);
        var hashBytes = md5.ComputeHash(bytes);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }

    // Buscar usuario por nombre
    public static Usuario? BuscarPorNombre(SistemaGestionLlaves.Data.ApplicationDbContext db, string nombre)
    {
        return db.Usuarios.FirstOrDefault(u => u.NombreUsuario == nombre);
    }

    // Crear usuario
    public static void CrearUsuario(SistemaGestionLlaves.Data.ApplicationDbContext db, string nombre, string password, int idRol, int idPersona)
    {
        var usuario = new Usuario
        {
            NombreUsuario = nombre,
            PasswordHash = CalcularMD5(password),
            IdRol = idRol,
            IdPersona = idPersona
        };
        db.Usuarios.Add(usuario);
        db.SaveChanges();
    }
}
