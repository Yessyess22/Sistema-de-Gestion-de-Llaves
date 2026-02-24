using Npgsql;
using SistemaWeb.Data;
using SistemaWeb.Services;

var builder = WebApplication.CreateBuilder(args);

// ==================== CONFIGURACIÓN DE SERVICIOS (API) ====================

// Registrar controladores (API)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ==================== CONFIGURAR DbContext CON PostgreSQL ====================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    
    // Usar Npgsql para PostgreSQL
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        // Configuración adicional de Npgsql (opcional)
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelaySeconds: 5,
            errorCodesToAdd: null);

        // Configurar versión de PostgreSQL
        npgsqlOptions.SetPostgresVersion(new Version(12, 0));
    });

    // Habilitar detalle de errores en desarrollo
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// ==================== REGISTRAR SERVICIOS ====================

// Registrar IPersonaService con su implementación
builder.Services.AddScoped<IPersonaService, PersonaService>();

// ==================== CONFIGURAR LOGGING ====================
builder.Services.AddLogging(config =>
{
    config.ClearProviders();
    config.AddConsole();
    config.AddDebug();
    
    if (builder.Environment.IsDevelopment())
    {
        config.SetMinimumLevel(LogLevel.Information);
    }
    else
    {
        config.SetMinimumLevel(LogLevel.Warning);
    }
});



// ==================== CONSTRUIR LA APLICACIÓN ====================
var app = builder.Build();

// ==================== MIDDLEWARE PIPELINE ====================


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

// ==================== INICIAR APLICACIÓN ====================

app.Run();
