using Ambientes.Data.Context;
using Ambientes.Data.Repositories;
using Ambientes.Services.Implementations;
using Ambientes.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;

var builder = WebApplicationBuilder.CreateBuilder(args);

// Configurar Serilog para logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        "logs/ambientes-api-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// Obtener la cadena de conexión desde appsettings
var connectionString = builder.Configuration.GetConnectionString("PostgreSQL") 
    ?? throw new InvalidOperationException("La cadena de conexión 'PostgreSQL' no está configurada");

// Registrar DbContext con PostgreSQL
builder.Services.AddDbContext<AmbientesDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelaySeconds: 10,
            errorCodesToAdd: null);
    });
});

// Registrar Repositorio
builder.Services.AddScoped<AmbienteRepository>();

// Registrar Servicio
builder.Services.AddScoped<IAmbienteService, AmbienteService>();

// Agregar controladores
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // Personalizar respuesta de validación
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>());

            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(
                new
                {
                    mensaje = "Validación fallida",
                    errores = errors
                });
        };
    });

// Registrar Swagger/OpenAPI
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "API de Ambientes",
            Version = "v1.0",
            Description = "Web API para gestionar ambientes con arquitectura en capas",
            Contact = new OpenApiContact
            {
                Name = "Desarrollo",
                Email = "dev@example.com"
            }
        });

    // Incluir comentarios XML
    var xmlFile = "Ambientes.API.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Agregar CORS si es necesario
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Agregar Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AmbientesDbContext>();

var app = builder.CreateBuilder();

// Aplicar migraciones automáticamente si la BD no existe
try
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<AmbientesDbContext>();
        
        Log.Information("Aplicando migraciones a la base de datos...");
        
        // Crear la BD si no existe
        await context.Database.EnsureCreatedAsync();
        
        Log.Information("Base de datos lista");
    }
}
catch (Exception ex)
{
    Log.Fatal(ex, "Error crítico al inicializar la base de datos");
    throw;
}

// Configurar middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "API de Ambientes v1.0");
        options.RoutePrefix = "doc";
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// Health checks endpoint
app.MapHealthChecks("/health");

// Mapear controladores
app.MapControllers();

// Ruta raíz que muestra información de la API
app.MapGet("/", () =>
{
    return Results.Ok(new
    {
        mensaje = "API de Ambientes v1.0",
        documentacion = "/doc",
        health = "/health",
        endpoints = new
        {
            obtenerTodos = "GET /api/ambientes",
            obtenerPorId = "GET /api/ambientes/{id}",
            crear = "POST /api/ambientes",
            actualizar = "PUT /api/ambientes/{id}",
            eliminar = "DELETE /api/ambientes/{id}"
        }
    });
});

try
{
    Log.Information("Iniciando API de Ambientes");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Aplicación terminada inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}

/// <summary>
/// Extensión para crear el builder Web con Serilog pre-configurado
/// </summary>
public static class WebApplicationBuilderExtensions
{
    public static WebApplication CreateBuilder(this WebApplicationBuilder builder)
    {
        return builder.Build();
    }
}
