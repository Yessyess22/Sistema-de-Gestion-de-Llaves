using BCrypt.Net;
using SistemaGestionLlaves.Models;

namespace SistemaGestionLlaves.Data;

/// <summary>
/// Datos semilla iniciales del sistema.
/// Se ejecuta al arrancar si la BD está vacía.
/// </summary>
public static class SeedData
{
    public static async Task InicializarAsync(ApplicationDbContext context)
    {
        // ── Tipos de Ambiente ────────────────────────────────
        if (!context.TiposAmbiente.Any())
        {
            context.TiposAmbiente.AddRange(
                new TipoAmbiente { NombreTipo = "Oficina" },
                new TipoAmbiente { NombreTipo = "Laboratorio" },
                new TipoAmbiente { NombreTipo = "Depósito" },
                new TipoAmbiente { NombreTipo = "Área común" },
                new TipoAmbiente { NombreTipo = "Otro" }
            );
            await context.SaveChangesAsync();
        }

        // ── Roles ────────────────────────────────────────────
        if (!context.Roles.Any())
        {
            context.Roles.AddRange(
                new Rol
                {
                    NombreRol = "Administrador",
                    Descripcion = "Acceso total al sistema. Gestiona usuarios, llaves y configuración.",
                    Estado = "A"
                },
                new Rol
                {
                    NombreRol = "Operador",
                    Descripcion = "Registra préstamos, devoluciones y reservas de llaves.",
                    Estado = "A"
                },
                new Rol
                {
                    NombreRol = "Consultor",
                    Descripcion = "Solo puede consultar el estado de las llaves y reportes.",
                    Estado = "A"
                }
            );
            await context.SaveChangesAsync();
        }

        // ── Permisos básicos ─────────────────────────────────
        if (!context.Permisos.Any())
        {
            context.Permisos.AddRange(
                new Permiso { NombrePermiso = "ver_dashboard", Descripcion = "Ver tablero principal" },
                new Permiso { NombrePermiso = "gestionar_llaves", Descripcion = "Crear, editar llaves" },
                new Permiso { NombrePermiso = "gestionar_ambientes", Descripcion = "Crear, editar ambientes" },
                new Permiso { NombrePermiso = "gestionar_personas", Descripcion = "Crear, editar personas" },
                new Permiso { NombrePermiso = "gestionar_usuarios", Descripcion = "Crear, editar usuarios del sistema" },
                new Permiso { NombrePermiso = "registrar_prestamo", Descripcion = "Registrar préstamo de llave" },
                new Permiso { NombrePermiso = "registrar_devolucion", Descripcion = "Registrar devolución de llave" },
                new Permiso { NombrePermiso = "gestionar_reservas", Descripcion = "Crear, cancelar reservas" },
                new Permiso { NombrePermiso = "ver_auditoria", Descripcion = "Ver registros de auditoría" },
                new Permiso { NombrePermiso = "ver_reportes", Descripcion = "Acceder a reportes" }
            );
            await context.SaveChangesAsync();
        }

        // ── Usuario Administrador por defecto ────────────────
        if (!context.Personas.Any())
        {
            var personaAdmin = new Persona
            {
                Nombres = "Administrador",
                Apellidos = "Sistema",
                Ci = "00000000",
                Correo = "admin@upds.edu.bo",
                Estado = "A"
            };
            context.Personas.Add(personaAdmin);
            await context.SaveChangesAsync();

            var rolAdmin = context.Roles.First(r => r.NombreRol == "Administrador");

            var usuarioAdmin = new Usuario
            {
                IdPersona = personaAdmin.IdPersona,
                IdRol = rolAdmin.IdRol,
                NombreUsuario = "admin",
                // Contraseña: Admin@1234 (hasheada con BCrypt)
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@1234"),
                FechaInicio = DateTime.UtcNow,
                Estado = "A"
            };
            context.Usuarios.Add(usuarioAdmin);
            await context.SaveChangesAsync();
        }
    }
}
