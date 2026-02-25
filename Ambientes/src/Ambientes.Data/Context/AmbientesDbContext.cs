using Ambientes.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Ambientes.Data.Context
{
    /// <summary>
    /// Contexto de base de datos para la entidad Ambiente.
    /// Configura la conexión a PostgreSQL y define las tablas del modelo.
    /// </summary>
    public class AmbientesDbContext : DbContext
    {
        public AmbientesDbContext(DbContextOptions<AmbientesDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// DbSet para la tabla de ambientes.
        /// </summary>
        public DbSet<Ambiente> Ambientes { get; set; }

        /// <summary>
        /// Configura el modelo de datos y las relaciones.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de índices para mejor rendimiento
            modelBuilder.Entity<Ambiente>()
                .HasIndex(a => a.Codigo)
                .IsUnique()
                .HasDatabaseName("idx_ambiente_codigo_unique");

            modelBuilder.Entity<Ambiente>()
                .HasIndex(a => a.Estado)
                .HasDatabaseName("idx_ambiente_estado");

            // Configuración de valores predeterminados
            modelBuilder.Entity<Ambiente>()
                .Property(a => a.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<Ambiente>()
                .Property(a => a.FechaActualizacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        }
    }
}
