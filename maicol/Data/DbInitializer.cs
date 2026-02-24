using SistemaWeb.Data;
using SistemaWeb.Models;

namespace SistemaWeb.Data
{
    /// <summary>
    /// Inicializador de base de datos.
    /// Verifica la conexión y realiza validaciones.
    /// </summary>
    public static class DbInitializer
    {
        /// <summary>
        /// Inicializa la base de datos al arrancar la aplicación.
        /// </summary>
        public static void Initialize(ApplicationDbContext context, ILogger<Program> logger)
        {
            try
            {
                // Verificar que la base de datos existe y es accesible
                if (context.Database.CanConnect())
                {
                    logger.LogInformation("✅ Conexión a la base de datos establecida correctamente");

                    // Verificar que las tablas existen
                    var personasCount = context.Personas.Count();
                    logger.LogInformation($"📊 Total de personas en la base de datos: {personasCount}");
                }
                else
                {
                    logger.LogError("❌ No se puede conectar a la base de datos");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Error al inicializar la base de datos: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Inserta datos de prueba (opcional, solo para desarrollo).
        /// </summary>
        public static void SeedData(ApplicationDbContext context)
        {
            // Si ya hay datos, no insertar más
            if (context.Personas.Any())
                return;

            var personas = new List<Persona>
            {
                new Persona
                {
                    Nombres = "Juan Carlos",
                    ApellidoPaterno = "García",
                    ApellidoMaterno = "López",
                    Email = "juan.garcia@ejemplo.com",
                    Telefono = "+591 76543210",
                    FechaNac = new DateTime(1990, 5, 15),
                    Tipo = "Documento",
                    Codigo = "PERS001",
                    CI = "1234567"
                },
                new Persona
                {
                    Nombres = "María Elena",
                    ApellidoPaterno = "Rodríguez",
                    ApellidoMaterno = "Martínez",
                    Email = "maria.rodriguez@ejemplo.com",
                    Telefono = "+591 71234567",
                    FechaNac = new DateTime(1992, 8, 22),
                    Tipo = "Documento",
                    Codigo = "PERS002",
                    CI = "1234568"
                },
                new Persona
                {
                    Nombres = "Tech Solutions S.A.",
                    ApellidoPaterno = "Solutions",
                    ApellidoMaterno = "Tech",
                    Email = "info@techsolutions.com",
                    Telefono = "+591 76666666",
                    FechaNac = new DateTime(2010, 1, 1),
                    Tipo = "Empresa",
                    Codigo = "EMP001",
                    CI = "NIT001"
                }
            };

            context.Personas.AddRange(personas);
            context.SaveChanges();
        }
    }
}
