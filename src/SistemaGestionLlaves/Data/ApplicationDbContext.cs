using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SistemaGestionLlaves.Models;

namespace SistemaGestionLlaves.Data;

/// <summary>
/// DbContext principal del Sistema de Gestión de Llaves.
/// Configura todas las entidades, relaciones e índices con Fluent API.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    // -------------------------------------------------------
    // DbSets
    // -------------------------------------------------------
    public DbSet<Persona> Personas => Set<Persona>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<TipoAmbiente> TiposAmbiente => Set<TipoAmbiente>();
    public DbSet<Ambiente> Ambientes => Set<Ambiente>();
    public DbSet<Llave> Llaves => Set<Llave>();
    public DbSet<Prestamo> Prestamos => Set<Prestamo>();
    public DbSet<Reserva> Reservas => Set<Reserva>();
    public DbSet<Auditoria> AuditoriaLog => Set<Auditoria>();
    public DbSet<Incidencia> Incidencias => Set<Incidencia>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var auditEntries = OnBeforeSaveChanges();
        var result = await base.SaveChangesAsync(cancellationToken);
        await OnAfterSaveChanges(auditEntries);
        return result;
    }

    private List<AuditEntry> OnBeforeSaveChanges()
    {
        ChangeTracker.DetectChanges();
        var auditEntries = new List<AuditEntry>();
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is Auditoria || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            var auditEntry = new AuditEntry(entry);
            auditEntry.TableName = entry.Entity.GetType().Name;
            // auditEntry.UserId = ... (Se podría pasar desde el exterior si se inyecta IHttpContextAccessor)
            auditEntries.Add(auditEntry);

            foreach (var property in entry.Properties)
            {
                string propertyName = property.Metadata.Name;
                if (property.Metadata.IsPrimaryKey())
                {
                    auditEntry.KeyValues[propertyName] = property.CurrentValue;
                    continue;
                }

                switch (entry.State)
                {
                    case EntityState.Added:
                        auditEntry.Action = "Create";
                        auditEntry.NewValues[propertyName] = property.CurrentValue;
                        break;

                    case EntityState.Deleted:
                        auditEntry.Action = "Delete";
                        auditEntry.OldValues[propertyName] = property.OriginalValue;
                        break;

                    case EntityState.Modified:
                        if (property.IsModified)
                        {
                            auditEntry.ChangedColumns.Add(propertyName);
                            auditEntry.Action = "Update";
                            auditEntry.OldValues[propertyName] = property.OriginalValue;
                            auditEntry.NewValues[propertyName] = property.CurrentValue;
                        }
                        break;
                }
            }
        }

        foreach (var auditEntry in auditEntries)
        {
            AuditoriaLog.Add(auditEntry.ToAudit());
        }

        return auditEntries;
    }

    private Task OnAfterSaveChanges(List<AuditEntry> auditEntries)
    {
        // Si quisiéramos manejar IDs temporales (que se generan al guardar), 
        // aquí es donde actualizaríamos el PrimaryKey del log.
        return Task.CompletedTask;
    }

    // ── Fluent API ──
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Auditoria ────────────────────────────────────────
        modelBuilder.Entity<Auditoria>(e =>
        {
            e.ToTable("Auditoria");
            e.HasKey(a => a.Id);
            e.Property(a => a.Id).HasColumnName("id").UseIdentityByDefaultColumn();
            e.Property(a => a.UserId).HasColumnName("user_id").HasMaxLength(150);
            e.Property(a => a.Type).HasColumnName("tipo_accion").HasMaxLength(50);
            e.Property(a => a.TableName).HasColumnName("tabla").HasMaxLength(150);
            e.Property(a => a.DateTime).HasColumnName("fecha_hora");
            e.Property(a => a.OldValues).HasColumnName("valores_anteriores").HasColumnType("text");
            e.Property(a => a.NewValues).HasColumnName("valores_nuevos").HasColumnType("text");
            e.Property(a => a.AffectedColumns).HasColumnName("columnas_afectadas").HasColumnType("text");
            e.Property(a => a.PrimaryKey).HasColumnName("llave_primaria").HasMaxLength(250);
        });

        // ── Persona ──────────────────────────────────────────
        modelBuilder.Entity<Persona>(e =>
        {
            e.ToTable("Persona");
            e.HasKey(p => p.IdPersona);
            e.Property(p => p.IdPersona).HasColumnName("id_persona").UseIdentityByDefaultColumn();
            e.Property(p => p.Nombres).HasColumnName("nombres").HasMaxLength(100).IsRequired();
            e.Property(p => p.Apellidos).HasColumnName("apellidos").HasMaxLength(100).IsRequired();
            e.Property(p => p.Ci).HasColumnName("ci").HasMaxLength(20).IsRequired();
            e.Property(p => p.FechaNacimiento).HasColumnName("fecha_nacimiento");
            e.Property(p => p.Genero).HasColumnName("genero").HasMaxLength(1);
            e.Property(p => p.Correo).HasColumnName("correo").HasMaxLength(150);
            e.Property(p => p.Celular).HasColumnName("celular").HasMaxLength(20);
            e.Property(p => p.Estado).HasColumnName("estado").HasMaxLength(1).IsRequired().HasDefaultValue("A");
            e.HasIndex(p => p.Ci).IsUnique().HasDatabaseName("UQ_Persona_Ci");
            e.HasIndex(p => p.Correo).HasDatabaseName("IX_Persona_Correo");
        });

        // ── Rol ──────────────────────────────────────────────
        modelBuilder.Entity<Rol>(e =>
        {
            e.ToTable("Rol");
            e.HasKey(r => r.IdRol);
            e.Property(r => r.IdRol).HasColumnName("id_rol").UseIdentityByDefaultColumn();
            e.Property(r => r.NombreRol).HasColumnName("nombre_rol").HasMaxLength(80).IsRequired();
            e.Property(r => r.Descripcion).HasColumnName("descripcion").HasMaxLength(250);
            e.Property(r => r.Estado).HasColumnName("estado").HasMaxLength(1).IsRequired().HasDefaultValue("A");
            e.HasIndex(r => r.NombreRol).IsUnique().HasDatabaseName("UQ_Rol_Nombre");
        });

        // ── Usuario ──────────────────────────────────────────
        modelBuilder.Entity<Usuario>(e =>
        {
            e.ToTable("Usuario");
            e.HasKey(u => u.IdUsuario);
            e.Property(u => u.IdUsuario).HasColumnName("id_usuario").UseIdentityByDefaultColumn();
            e.Property(u => u.IdPersona).HasColumnName("id_persona");
            e.Property(u => u.IdRol).HasColumnName("id_rol");
            e.Property(u => u.NombreUsuario).HasColumnName("nombre_usuario").HasMaxLength(80).IsRequired();
            e.Property(u => u.PasswordHash).HasColumnName("password_hash").HasMaxLength(200).IsRequired();
            e.Property(u => u.FechaInicio).HasColumnName("fecha_inicio");
            e.Property(u => u.FechaFin).HasColumnName("fecha_fin");
            e.Property(u => u.Estado).HasColumnName("estado").HasMaxLength(1).IsRequired().HasDefaultValue("A");
            e.HasIndex(u => u.NombreUsuario).IsUnique().HasDatabaseName("UQ_Usuario_NombreUsuario");

            e.HasOne(u => u.Persona)
             .WithMany(p => p.Usuarios)
             .HasForeignKey(u => u.IdPersona)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(u => u.Rol)
             .WithMany(r => r.Usuarios)
             .HasForeignKey(u => u.IdRol)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── TipoAmbiente ─────────────────────────────────────
        modelBuilder.Entity<TipoAmbiente>(e =>
        {
            e.ToTable("TipoAmbiente");
            e.HasKey(t => t.IdTipo);
            e.Property(t => t.IdTipo).HasColumnName("id_tipo").UseIdentityByDefaultColumn();
            e.Property(t => t.NombreTipo).HasColumnName("nombre_tipo").HasMaxLength(80).IsRequired();
            e.HasIndex(t => t.NombreTipo).IsUnique().HasDatabaseName("UQ_TipoAmbiente_Nombre");
        });

        // ── Ambiente ─────────────────────────────────────────
        modelBuilder.Entity<Ambiente>(e =>
        {
            e.ToTable("Ambiente");
            e.HasKey(a => a.IdAmbiente);
            e.Property(a => a.IdAmbiente).HasColumnName("id_ambiente").UseIdentityByDefaultColumn();
            e.Property(a => a.Codigo).HasColumnName("codigo").HasMaxLength(20).IsRequired();
            e.Property(a => a.Nombre).HasColumnName("nombre").HasMaxLength(150).IsRequired();
            e.Property(a => a.Ubicacion).HasColumnName("ubicacion").HasMaxLength(200);
            e.Property(a => a.IdTipo).HasColumnName("id_tipo");
            e.Property(a => a.Estado).HasColumnName("estado").HasMaxLength(1).IsRequired().HasDefaultValue("A");
            e.HasIndex(a => a.Codigo).IsUnique().HasDatabaseName("UQ_Ambiente_Codigo");

            e.HasOne(a => a.TipoAmbiente)
             .WithMany(t => t.Ambientes)
             .HasForeignKey(a => a.IdTipo)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Llave ─────────────────────────────────────────────
        modelBuilder.Entity<Llave>(e =>
        {
            e.ToTable("Llave");
            e.HasKey(l => l.IdLlave);
            e.Property(l => l.IdLlave).HasColumnName("id_llave").UseIdentityByDefaultColumn();
            e.Property(l => l.Codigo).HasColumnName("codigo").HasMaxLength(30).IsRequired();
            e.Property(l => l.NumCopias).HasColumnName("num_copias").HasDefaultValue(1);
            e.Property(l => l.IdAmbiente).HasColumnName("id_ambiente");
            e.Property(l => l.EsMaestra).HasColumnName("es_maestra").HasDefaultValue(false);
            e.Property(l => l.Estado).HasColumnName("estado").HasMaxLength(1).IsRequired().HasDefaultValue("D");
            e.Property(l => l.Observaciones).HasColumnName("observaciones").HasMaxLength(300);
            e.HasIndex(l => l.Codigo).IsUnique().HasDatabaseName("UQ_Llave_Codigo");

            e.HasOne(l => l.Ambiente)
             .WithMany(a => a.Llaves)
             .HasForeignKey(l => l.IdAmbiente)
             .OnDelete(DeleteBehavior.Restrict);
        });


        // ── Prestamo ─────────────────────────────────────────
        modelBuilder.Entity<Prestamo>(e =>
        {
            e.ToTable("Prestamo");
            e.HasKey(p => p.IdPrestamo);
            e.Property(p => p.IdPrestamo).HasColumnName("id_prestamo").UseIdentityByDefaultColumn();
            e.Property(p => p.IdLlave).HasColumnName("id_llave");
            e.Property(p => p.IdPersona).HasColumnName("id_persona");
            e.Property(p => p.IdUsuario).HasColumnName("id_usuario");
            e.Property(p => p.FechaHoraPrestamo).HasColumnName("fecha_hora_prestamo").HasDefaultValueSql("NOW()");
            e.Property(p => p.FechaHoraDevolucionEsperada).HasColumnName("fecha_hora_devolucion_esperada");
            e.Property(p => p.FechaHoraDevolucionReal).HasColumnName("fecha_hora_devolucion_real");
            e.Property(p => p.Estado).HasColumnName("estado").HasMaxLength(1).IsRequired().HasDefaultValue("A");
            e.Property(p => p.Observaciones).HasColumnName("observaciones").HasMaxLength(300);
            e.Property(p => p.FirmaBase64).HasColumnName("firma_base64").HasColumnType("text");
            e.HasIndex(p => p.Estado).HasDatabaseName("IX_Prestamo_Estado");
            e.HasIndex(p => p.FechaHoraPrestamo).HasDatabaseName("IX_Prestamo_Fecha");

            e.HasOne(p => p.Llave)
             .WithMany(l => l.Prestamos)
             .HasForeignKey(p => p.IdLlave)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(p => p.Persona)
             .WithMany(ps => ps.Prestamos)
             .HasForeignKey(p => p.IdPersona)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(p => p.Usuario)
             .WithMany(u => u.Prestamos)
             .HasForeignKey(p => p.IdUsuario)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Reserva ─────────────────────────────────────────
        modelBuilder.Entity<Reserva>(e =>
        {
            e.ToTable("Reserva");
            e.HasKey(r => r.IdReserva);
            e.Property(r => r.IdReserva).HasColumnName("id_reserva").UseIdentityByDefaultColumn();
            e.Property(r => r.IdLlave).HasColumnName("id_llave");
            e.Property(r => r.IdPersona).HasColumnName("id_persona");
            e.Property(r => r.IdUsuario).HasColumnName("id_usuario");
            e.Property(r => r.FechaInicio).HasColumnName("fecha_inicio");
            e.Property(r => r.FechaFin).HasColumnName("fecha_fin");
            e.Property(r => r.Estado).HasColumnName("estado").HasMaxLength(1).IsRequired().HasDefaultValue("P");
            e.HasIndex(r => r.Estado).HasDatabaseName("IX_Reserva_Estado");

            e.HasOne(r => r.Llave)
             .WithMany(l => l.Reservas)
             .HasForeignKey(r => r.IdLlave)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(r => r.Persona)
             .WithMany(p => p.Reservas)
             .HasForeignKey(r => r.IdPersona)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(r => r.Usuario)
             .WithMany(u => u.Reservas)
             .HasForeignKey(r => r.IdUsuario)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Incidencia ──────────────────────────────────────
        modelBuilder.Entity<Incidencia>(e =>
        {
            e.ToTable("Incidencia");
            e.HasKey(i => i.IdIncidencia);
            e.Property(i => i.IdIncidencia).HasColumnName("id_incidencia").UseIdentityByDefaultColumn();
            e.Property(i => i.IdLlave).HasColumnName("id_llave");
            e.Property(i => i.TipoIncidencia).HasColumnName("tipo_incidencia").HasMaxLength(50).IsRequired();
            e.Property(i => i.Descripcion).HasColumnName("descripcion").HasMaxLength(500).IsRequired();
            e.Property(i => i.FechaReporte).HasColumnName("fecha_reporte").HasDefaultValueSql("NOW()");
            e.Property(i => i.FechaResolucion).HasColumnName("fecha_resolucion");
            e.Property(i => i.Estado).HasColumnName("estado").HasMaxLength(1).IsRequired().HasDefaultValue("A");
            e.Property(i => i.NotasResolucion).HasColumnName("notas_resolucion").HasMaxLength(200);

            e.HasOne(i => i.Llave)
             .WithMany(l => l.Incidencias)
             .HasForeignKey(i => i.IdLlave)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
