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
    public DbSet<Permiso> Permisos => Set<Permiso>();
    public DbSet<RolPermiso> RolPermisos => Set<RolPermiso>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<TipoAmbiente> TiposAmbiente => Set<TipoAmbiente>();
    public DbSet<Ambiente> Ambientes => Set<Ambiente>();
    public DbSet<Llave> Llaves => Set<Llave>();
    public DbSet<PersonaAutorizada> PersonasAutorizadas => Set<PersonaAutorizada>();
    public DbSet<Prestamo> Prestamos => Set<Prestamo>();
    public DbSet<Reserva> Reservas => Set<Reserva>();
    public DbSet<Auditoria> Auditorias => Set<Auditoria>();
    public DbSet<IntentoAcceso> IntentosAcceso => Set<IntentoAcceso>();
    public DbSet<AlertaNotificacion> AlertasNotificaciones => Set<AlertaNotificacion>();

    // -------------------------------------------------------
    // Fluent API
    // -------------------------------------------------------
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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

        // ── Permiso ──────────────────────────────────────────
        modelBuilder.Entity<Permiso>(e =>
        {
            e.ToTable("Permisos");
            e.HasKey(p => p.IdPermiso);
            e.Property(p => p.IdPermiso).HasColumnName("id_permiso").UseIdentityByDefaultColumn();
            e.Property(p => p.NombrePermiso).HasColumnName("nombre_permiso").HasMaxLength(100).IsRequired();
            e.Property(p => p.Descripcion).HasColumnName("descripcion").HasMaxLength(250);
            e.HasIndex(p => p.NombrePermiso).IsUnique().HasDatabaseName("UQ_Permiso_Nombre");
        });

        // ── RolPermiso (tabla intermedia) ────────────────────
        modelBuilder.Entity<RolPermiso>(e =>
        {
            e.ToTable("RolPermisos");
            e.HasKey(rp => new { rp.IdRol, rp.IdPermiso });
            e.Property(rp => rp.IdRol).HasColumnName("id_rol");
            e.Property(rp => rp.IdPermiso).HasColumnName("id_permiso");

            e.HasOne(rp => rp.Rol)
             .WithMany(r => r.RolPermisos)
             .HasForeignKey(rp => rp.IdRol)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(rp => rp.Permiso)
             .WithMany(p => p.RolPermisos)
             .HasForeignKey(rp => rp.IdPermiso)
             .OnDelete(DeleteBehavior.Cascade);
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

        // ── PersonaAutorizada ─────────────────────────────────
        modelBuilder.Entity<PersonaAutorizada>(e =>
        {
            e.ToTable("Persona_Autorizada");
            e.HasKey(pa => pa.Id);
            e.Property(pa => pa.Id).HasColumnName("id").UseIdentityByDefaultColumn();
            e.Property(pa => pa.IdPersona).HasColumnName("id_persona");
            e.Property(pa => pa.IdLlave).HasColumnName("id_llave");
            e.HasIndex(pa => new { pa.IdPersona, pa.IdLlave }).IsUnique()
             .HasDatabaseName("UQ_PersonaAutorizada_PersonaLlave");

            e.HasOne(pa => pa.Persona)
             .WithMany(p => p.PersonasAutorizadas)
             .HasForeignKey(pa => pa.IdPersona)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(pa => pa.Llave)
             .WithMany(l => l.PersonasAutorizadas)
             .HasForeignKey(pa => pa.IdLlave)
             .OnDelete(DeleteBehavior.Cascade);
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

        // ── Auditoria ────────────────────────────────────────
        modelBuilder.Entity<Auditoria>(e =>
        {
            e.ToTable("Auditoria");
            e.HasKey(a => a.IdAuditoria);
            e.Property(a => a.IdAuditoria).HasColumnName("id_auditoria").UseIdentityByDefaultColumn();
            e.Property(a => a.TablaAfectada).HasColumnName("tabla_afectada").HasMaxLength(100).IsRequired();
            e.Property(a => a.Operacion).HasColumnName("operacion").HasMaxLength(20).IsRequired();
            e.Property(a => a.IdRegistro).HasColumnName("id_registro");
            e.Property(a => a.IdUsuario).HasColumnName("id_usuario");
            e.Property(a => a.FechaHora).HasColumnName("fecha_hora").HasDefaultValueSql("NOW()");
            e.Property(a => a.DatosAnteriores).HasColumnName("datos_anteriores").HasColumnType("text");
            e.Property(a => a.DatosNuevos).HasColumnName("datos_nuevos").HasColumnType("text");
            e.HasIndex(a => a.FechaHora).HasDatabaseName("IX_Auditoria_Fecha");
            e.HasIndex(a => a.TablaAfectada).HasDatabaseName("IX_Auditoria_Tabla");

            e.HasOne(a => a.Usuario)
             .WithMany(u => u.Auditorias)
             .HasForeignKey(a => a.IdUsuario)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── IntentoAcceso ────────────────────────────────────
        modelBuilder.Entity<IntentoAcceso>(e =>
        {
            e.ToTable("IntentoAcceso");
            e.HasKey(i => i.IdIntento);
            e.Property(i => i.IdIntento).HasColumnName("id_intento").UseIdentityByDefaultColumn();
            e.Property(i => i.NombreUsuario).HasColumnName("nombre_usuario").HasMaxLength(80).IsRequired();
            e.Property(i => i.FechaHora).HasColumnName("fecha_hora").HasDefaultValueSql("NOW()");
            e.Property(i => i.Ip).HasColumnName("ip").HasMaxLength(50);
            e.Property(i => i.Exitoso).HasColumnName("exitoso").HasDefaultValue(false);
            e.HasIndex(i => i.FechaHora).HasDatabaseName("IX_IntentoAcceso_Fecha");
        });

        // ── AlertaNotificacion ───────────────────────────────
        modelBuilder.Entity<AlertaNotificacion>(e =>
        {
            e.ToTable("AlertaNotificacion");
            e.HasKey(a => a.IdAlerta);
            e.Property(a => a.IdAlerta).HasColumnName("id_alerta").UseIdentityByDefaultColumn();
            e.Property(a => a.TipoAlerta).HasColumnName("tipo_alerta").HasMaxLength(50).IsRequired();
            e.Property(a => a.IdPrestamo).HasColumnName("id_prestamo");
            e.Property(a => a.IdLlave).HasColumnName("id_llave");
            e.Property(a => a.Mensaje).HasColumnName("mensaje").HasMaxLength(500).IsRequired();
            e.Property(a => a.FechaGenerada).HasColumnName("fecha_generada").HasDefaultValueSql("NOW()");
            e.Property(a => a.Leida).HasColumnName("leida").HasDefaultValue(false);
            e.HasIndex(a => a.Leida).HasDatabaseName("IX_Alerta_Leida");

            e.HasOne(a => a.Prestamo)
             .WithMany(p => p.Alertas)
             .HasForeignKey(a => a.IdPrestamo)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(a => a.Llave)
             .WithMany(l => l.Alertas)
             .HasForeignKey(a => a.IdLlave)
             .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
