using Microsoft.EntityFrameworkCore;
using Moq;
using SistemaWeb.Data;
using SistemaWeb.Models;
using SistemaWeb.Services;

namespace SistemaWeb.Tests
{
    /// <summary>
    /// Pruebas unitarias para PersonaService.
    /// Requiere: dotnet add package xunit
    ///           dotnet add package Moq
    /// </summary>
    public class PersonaServiceTests
    {
        /// <summary>
        /// Crea un contexto de BD en memoria para pruebas.
        /// </summary>
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            return new ApplicationDbContext(options);
        }

        /// <summary>
        /// Crea un logger mock para pruebas.
        /// </summary>
        private ILogger<PersonaService> GetMockLogger()
        {
            return new Mock<ILogger<PersonaService>>().Object;
        }

        #region Pruebas de Obtener

        /// <summary>
        /// Prueba: Obtener todas las personas correctamente.
        /// </summary>
        [Fact]
        public async Task ObtenerTodas_DebeRetornarTodasLasPersonas()
        {
            // Arrange
            var context = GetDbContext();
            var logger = GetMockLogger();
            var service = new PersonaService(context, logger);

            var persona1 = new Persona
            {
                Nombres = "Juan",
                ApellidoPaterno = "García",
                ApellidoMaterno = "López",
                Email = "juan@ejemplo.com",
                Telefono = "+591 76543210",
                FechaNac = new DateTime(1990, 1, 1),
                Tipo = "Documento",
                Codigo = "PERS001",
                CI = "1234567"
            };

            var persona2 = new Persona
            {
                Nombres = "María",
                ApellidoPaterno = "Rodríguez",
                ApellidoMaterno = "Martínez",
                Email = "maria@ejemplo.com",
                Telefono = "+591 71234567",
                FechaNac = new DateTime(1992, 1, 1),
                Tipo = "Documento",
                Codigo = "PERS002",
                CI = "1234568"
            };

            context.Personas.AddRange(persona1, persona2);
            await context.SaveChangesAsync();

            // Act
            var resultado = await service.ObtenerTodasAsync();

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(2, resultado.Count());
        }

        /// <summary>
        /// Prueba: Obtener persona por ID correctamente.
        /// </summary>
        [Fact]
        public async Task ObtenerPorId_DebeRetornarPersona()
        {
            // Arrange
            var context = GetDbContext();
            var logger = GetMockLogger();
            var service = new PersonaService(context, logger);

            var persona = new Persona
            {
                Nombres = "Test",
                ApellidoPaterno = "Test",
                ApellidoMaterno = "Test",
                Email = "test@ejemplo.com",
                Telefono = "+591 70000000",
                FechaNac = new DateTime(1990, 1, 1),
                Tipo = "Documento",
                Codigo = "TEST001",
                CI = "9999999"
            };

            context.Personas.Add(persona);
            await context.SaveChangesAsync();

            // Act
            var resultado = await service.ObtenerPorIdAsync(persona.IdPersona);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal("Test", resultado.Nombres);
        }

        /// <summary>
        /// Prueba: Retorna null si persona no existe.
        /// </summary>
        [Fact]
        public async Task ObtenerPorId_DebeRetornarNullSiNoExiste()
        {
            // Arrange
            var context = GetDbContext();
            var logger = GetMockLogger();
            var service = new PersonaService(context, logger);

            // Act
            var resultado = await service.ObtenerPorIdAsync(9999);

            // Assert
            Assert.Null(resultado);
        }

        #endregion

        #region Pruebas de Crear

        /// <summary>
        /// Prueba: Crear persona correctamente.
        /// </summary>
        [Fact]
        public async Task Crear_DebeCrearPersonaCorrectamente()
        {
            // Arrange
            var context = GetDbContext();
            var logger = GetMockLogger();
            var service = new PersonaService(context, logger);

            var persona = new Persona
            {
                Nombres = "Nueva",
                ApellidoPaterno = "Persona",
                ApellidoMaterno = "Test",
                Email = "nueva@ejemplo.com",
                Telefono = "+591 70000000",
                FechaNac = new DateTime(1990, 1, 1),
                Tipo = "Documento",
                Codigo = "NEW001",
                CI = "8888888"
            };

            // Act
            var resultado = await service.CrearAsync(persona);

            // Assert
            Assert.NotNull(resultado);
            Assert.True(resultado.IdPersona > 0);
            Assert.Equal(1, context.Personas.Count());
        }

        /// <summary>
        /// Prueba: No permite crear persona con email duplicado.
        /// </summary>
        [Fact]
        public async Task Crear_DebeRechazarEmailDuplicado()
        {
            // Arrange
            var context = GetDbContext();
            var logger = GetMockLogger();
            var service = new PersonaService(context, logger);

            var persona1 = new Persona
            {
                Nombres = "Persona1",
                ApellidoPaterno = "Test",
                ApellidoMaterno = "Test",
                Email = "duplicado@ejemplo.com",
                Telefono = "+591 70000000",
                FechaNac = new DateTime(1990, 1, 1),
                Tipo = "Documento",
                Codigo = "DUP001",
                CI = "1111111"
            };

            var persona2 = new Persona
            {
                Nombres = "Persona2",
                ApellidoPaterno = "Test",
                ApellidoMaterno = "Test",
                Email = "duplicado@ejemplo.com", // Email igual
                Telefono = "+591 71111111",
                FechaNac = new DateTime(1990, 1, 1),
                Tipo = "Documento",
                Codigo = "DUP002",
                CI = "1111112"
            };

            await service.CrearAsync(persona1);

            // Act & Assert
            var excepcion = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await service.CrearAsync(persona2)
            );

            Assert.Contains("Ya existe una persona con el correo", excepcion.Message);
        }

        /// <summary>
        /// Prueba: No permite crear persona con CI duplicado.
        /// </summary>
        [Fact]
        public async Task Crear_DebeRechazarCIDuplicado()
        {
            // Arrange
            var context = GetDbContext();
            var logger = GetMockLogger();
            var service = new PersonaService(context, logger);

            var persona1 = new Persona
            {
                Nombres = "Persona1",
                ApellidoPaterno = "Test",
                ApellidoMaterno = "Test",
                Email = "p1@ejemplo.com",
                Telefono = "+591 70000000",
                FechaNac = new DateTime(1990, 1, 1),
                Tipo = "Documento",
                Codigo = "CI001",
                CI = "9876543"
            };

            var persona2 = new Persona
            {
                Nombres = "Persona2",
                ApellidoPaterno = "Test",
                ApellidoMaterno = "Test",
                Email = "p2@ejemplo.com",
                Telefono = "+591 71111111",
                FechaNac = new DateTime(1990, 1, 1),
                Tipo = "Documento",
                Codigo = "CI002",
                CI = "9876543" // CI igual
            };

            await service.CrearAsync(persona1);

            // Act & Assert
            var excepcion = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await service.CrearAsync(persona2)
            );

            Assert.Contains("Ya existe una persona con el CI", excepcion.Message);
        }

        #endregion

        #region Pruebas de Actualizar

        /// <summary>
        /// Prueba: Actualizar persona correctamente.
        /// </summary>
        [Fact]
        public async Task Actualizar_DebeActualizarPersonaCorrectamente()
        {
            // Arrange
            var context = GetDbContext();
            var logger = GetMockLogger();
            var service = new PersonaService(context, logger);

            var persona = new Persona
            {
                Nombres = "Juan",
                ApellidoPaterno = "García",
                ApellidoMaterno = "López",
                Email = "juan@ejemplo.com",
                Telefono = "+591 76543210",
                FechaNac = new DateTime(1990, 1, 1),
                Tipo = "Documento",
                Codigo = "UPD001",
                CI = "7777777"
            };

            await service.CrearAsync(persona);
            persona.Nombres = "Carlos"; // Cambiar nombre

            // Act
            var resultado = await service.ActualizarAsync(persona);

            // Assert
            Assert.Equal("Carlos", resultado.Nombres);
        }

        /// <summary>
        /// Prueba: No permite actualizar persona inexistente.
        /// </summary>
        [Fact]
        public async Task Actualizar_DebeRechazarPersonaInexistente()
        {
            // Arrange
            var context = GetDbContext();
            var logger = GetMockLogger();
            var service = new PersonaService(context, logger);

            var persona = new Persona
            {
                IdPersona = 9999,
                Nombres = "NoExiste",
                ApellidoPaterno = "Test",
                ApellidoMaterno = "Test",
                Email = "noexiste@ejemplo.com",
                Telefono = "+591 70000000",
                FechaNac = new DateTime(1990, 1, 1),
                Tipo = "Documento",
                Codigo = "NEX001",
                CI = "6666666"
            };

            // Act & Assert
            var excepcion = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await service.ActualizarAsync(persona)
            );

            Assert.Contains("No se encontró la persona", excepcion.Message);
        }

        #endregion

        #region Pruebas de Eliminar

        /// <summary>
        /// Prueba: Eliminar persona correctamente.
        /// </summary>
        [Fact]
        public async Task Eliminar_DebeEliminarPersonaCorrectamente()
        {
            // Arrange
            var context = GetDbContext();
            var logger = GetMockLogger();
            var service = new PersonaService(context, logger);

            var persona = new Persona
            {
                Nombres = "AEliminar",
                ApellidoPaterno = "Test",
                ApellidoMaterno = "Test",
                Email = "aeliminar@ejemplo.com",
                Telefono = "+591 70000000",
                FechaNac = new DateTime(1990, 1, 1),
                Tipo = "Documento",
                Codigo = "DEL001",
                CI = "5555555"
            };

            await service.CrearAsync(persona);

            // Act
            var resultado = await service.EliminarAsync(persona.IdPersona);

            // Assert
            Assert.True(resultado);
            Assert.Equal(0, context.Personas.Count());
        }

        #endregion

        #region Pruebas de Búsqueda

        /// <summary>
        /// Prueba: Buscar personas por nombre.
        /// </summary>
        [Fact]
        public async Task BuscarPorNombre_DebeEncontrarPersonas()
        {
            // Arrange
            var context = GetDbContext();
            var logger = GetMockLogger();
            var service = new PersonaService(context, logger);

            var persona = new Persona
            {
                Nombres = "Juan",
                ApellidoPaterno = "García",
                ApellidoMaterno = "López",
                Email = "juan@ejemplo.com",
                Telefono = "+591 76543210",
                FechaNac = new DateTime(1990, 1, 1),
                Tipo = "Documento",
                Codigo = "BUSCA001",
                CI = "4444444"
            };

            await service.CrearAsync(persona);

            // Act
            var resultado = await service.BuscarPorNombreAsync("Juan");

            // Assert
            Assert.NotNull(resultado);
            Assert.Single(resultado);
        }

        /// <summary>
        /// Prueba: Obtener personas por tipo.
        /// </summary>
        [Fact]
        public async Task ObtenerPorTipo_DebeFilterarCorrectamente()
        {
            // Arrange
            var context = GetDbContext();
            var logger = GetMockLogger();
            var service = new PersonaService(context, logger);

            var persona1 = new Persona
            {
                Nombres = "Persona",
                ApellidoPaterno = "Test",
                ApellidoMaterno = "Test",
                Email = "p1@ejemplo.com",
                Telefono = "+591 70000000",
                FechaNac = new DateTime(1990, 1, 1),
                Tipo = "Documento",
                Codigo = "TIP001",
                CI = "3333333"
            };

            var persona2 = new Persona
            {
                Nombres = "Empresa",
                ApellidoPaterno = "Test",
                ApellidoMaterno = "Test",
                Email = "empresa@ejemplo.com",
                Telefono = "+591 71111111",
                FechaNac = new DateTime(2010, 1, 1),
                Tipo = "Empresa",
                Codigo = "TIP002",
                CI = "3333334"
            };

            await service.CrearAsync(persona1);
            await service.CrearAsync(persona2);

            // Act
            var resultadoDocumentos = await service.ObtenerPorTipoAsync("Documento");
            var resultadoEmpresas = await service.ObtenerPorTipoAsync("Empresa");

            // Assert
            Assert.Single(resultadoDocumentos);
            Assert.Single(resultadoEmpresas);
        }

        #endregion
    }
}
