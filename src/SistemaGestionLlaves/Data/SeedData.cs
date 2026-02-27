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
        // Se verifica por nombre de usuario, no por "si hay alguna persona",
        // para que el seeder sea idempotente en cualquier estado de la BD.
        const string adminUsername = "admin";
        const string adminPassword = "password";

        var usuarioAdminExistente = context.Usuarios
            .FirstOrDefault(u => u.NombreUsuario == adminUsername);

        if (usuarioAdminExistente == null)
        {
            // Buscar o crear la Persona del admin
            var personaAdmin = context.Personas.FirstOrDefault(p => p.Ci == "00000000")
                ?? new Persona
                {
                    Nombres  = "Administrador",
                    Apellidos = "Sistema",
                    Ci       = "00000000",
                    Correo   = "admin@upds.edu.bo",
                    Estado   = "A"
                };

            if (personaAdmin.IdPersona == 0)
            {
                context.Personas.Add(personaAdmin);
                await context.SaveChangesAsync();
            }

            var rolAdmin = context.Roles.First(r => r.NombreRol == "Administrador");

            context.Usuarios.Add(new Usuario
            {
                IdPersona    = personaAdmin.IdPersona,
                IdRol        = rolAdmin.IdRol,
                NombreUsuario = adminUsername,
                PasswordHash  = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                FechaInicio   = DateTime.UtcNow,
                Estado        = "A"
            });
            await context.SaveChangesAsync();
        }
        else if (!BCrypt.Net.BCrypt.Verify(adminPassword, usuarioAdminExistente.PasswordHash))
        {
            // El admin existe pero con un hash distinto → actualizar al hash correcto
            usuarioAdminExistente.PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword);
            await context.SaveChangesAsync();
        }

        // ── Ambientes de prueba ──────────────────────────────
        if (!context.Ambientes.Any())
        {
            var tipoOficina = context.TiposAmbiente.First(t => t.NombreTipo == "Oficina");
            var tipoLab     = context.TiposAmbiente.First(t => t.NombreTipo == "Laboratorio");
            var tipoCom     = context.TiposAmbiente.First(t => t.NombreTipo == "Área común");

            context.Ambientes.AddRange(
                new Ambiente { Codigo = "A-101", Nombre = "Oficina Administración",   Ubicacion = "Piso 1 - Bloque A", IdTipo = tipoOficina.IdTipo, Estado = "A" },
                new Ambiente { Codigo = "LAB-01", Nombre = "Laboratorio de Cómputo 1", Ubicacion = "Piso 2 - Bloque B", IdTipo = tipoLab.IdTipo,     Estado = "A" },
                new Ambiente { Codigo = "LAB-02", Nombre = "Laboratorio de Cómputo 2", Ubicacion = "Piso 2 - Bloque B", IdTipo = tipoLab.IdTipo,     Estado = "A" },
                new Ambiente { Codigo = "SAL-01", Nombre = "Sala de Reuniones",        Ubicacion = "Piso 1 - Bloque C", IdTipo = tipoCom.IdTipo,     Estado = "A" }
            );
            await context.SaveChangesAsync();
        }

        // ── Llaves de prueba ─────────────────────────────────
        if (!context.Llaves.Any())
        {
            var oficina = context.Ambientes.First(a => a.Codigo == "A-101");
            var lab1    = context.Ambientes.First(a => a.Codigo == "LAB-01");
            var lab2    = context.Ambientes.First(a => a.Codigo == "LAB-02");
            var sala    = context.Ambientes.First(a => a.Codigo == "SAL-01");

            context.Llaves.AddRange(
                new Llave { Codigo = "LL-ADM-01", NumCopias = 2, IdAmbiente = oficina.IdAmbiente, EsMaestra = true,  Estado = "D", Observaciones = "Llave maestra - Administración" },
                new Llave { Codigo = "LL-LAB1-01", NumCopias = 1, IdAmbiente = lab1.IdAmbiente,   EsMaestra = false, Estado = "D", Observaciones = "Laboratorio 1 - copia A" },
                new Llave { Codigo = "LL-LAB1-02", NumCopias = 1, IdAmbiente = lab1.IdAmbiente,   EsMaestra = false, Estado = "P", Observaciones = "Laboratorio 1 - copia B (prestada)" },
                new Llave { Codigo = "LL-LAB2-01", NumCopias = 1, IdAmbiente = lab2.IdAmbiente,   EsMaestra = false, Estado = "D" },
                new Llave { Codigo = "LL-SAL-01",  NumCopias = 3, IdAmbiente = sala.IdAmbiente,   EsMaestra = false, Estado = "D", Observaciones = "Sala de reuniones" },
                new Llave { Codigo = "LL-INAC-01", NumCopias = 1, IdAmbiente = oficina.IdAmbiente, EsMaestra = false, Estado = "I", Observaciones = "Fuera de servicio" }
            );
            await context.SaveChangesAsync();
        }
    }
}
