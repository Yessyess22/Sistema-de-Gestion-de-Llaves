using Microsoft.EntityFrameworkCore;
using SistemaWeb.Models;

namespace SistemaWeb.Data
{
    /// <summary>
    /// DbContext principal para la aplicación.
    /// Configura la conexión a PostgreSQL y mapea las entidades.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// DbSet para la tabla Personas
        /// </summary>
        public DbSet<Persona> Personas { get; set; } = null!;

        /// <summary>
        /// Configuración adicional del modelo (Fluent API)
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de la tabla Personas
            modelBuilder.Entity<Persona>(entity =>
            {
                // Clave primaria
                entity.HasKey(e => e.IdPersona);

                // Mapeo de propiedades a columnas
                entity.Property(e => e.IdPersona)
                    .HasColumnName("id_persona")
                    .IsRequired();

                entity.Property(e => e.Nombres)
                    .HasColumnName("nombres")
                    .HasColumnType("varchar")
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(e => e.ApellidoPaterno)
                    .HasColumnName("apellido_paterno")
                    .HasColumnType("varchar")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.ApellidoMaterno)
                    .HasColumnName("apellido_materno")
                    .HasColumnType("varchar")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.Email)
                    .HasColumnName("email")
                    .HasColumnType("varchar")
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(e => e.Telefono)
                    .HasColumnName("telefono")
                    .HasColumnType("varchar")
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(e => e.FechaNac)
                    .HasColumnName("fecha_nac")
                    .HasColumnType("date")
                    .IsRequired();

                entity.Property(e => e.Tipo)
                    .HasColumnName("tipo")
                    .HasColumnType("varchar")
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(e => e.Codigo)
                    .HasColumnName("codigo")
                    .HasColumnType("varchar")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.CI)
                    .HasColumnName("ci")
                    .HasColumnType("varchar")
                    .HasMaxLength(30)
                    .IsRequired();

                // Índices para mejorar rendimiento
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.CI).IsUnique();
                entity.HasIndex(e => e.Codigo);
            });
        }
    }
}
