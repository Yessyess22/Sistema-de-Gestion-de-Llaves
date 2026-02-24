# 🚀 GUÍA COMPLETA DE INSTALACIÓN Y CONFIGURACIÓN

## Módulo PERSONA - ASP.NET Core MVC 8 + PostgreSQL

---

## ✅ LISTA DE VERIFICACIÓN PRE-INSTALACIÓN

- [ ] .NET 8 SDK instalado
- [ ] PostgreSQL 12+ instalado y ejecutando
- [ ] Visual Studio 2022 o VS Code
- [ ] Git (opcional)
- [ ] Node.js (si usa npm para frontend)

---

## 📋 PASO 1: PREPARACIÓN DEL ENTORNO

### 1.1 Verificar .NET 8 instalado

```bash
dotnet --version
```

**Resultado esperado:** `8.0.x`

Si no está instalado, [descargue .NET 8](https://dotnet.microsoft.com/download/dotnet/8.0)

### 1.2 Verificar PostgreSQL instalado

```bash
psql --version
```

**Resultado esperado:** `psql (PostgreSQL) 12.x`

Si no está instalado, [descargue PostgreSQL](https://www.postgresql.org/download/)

### 1.3 Verificar que PostgreSQL está corriendo

**En Windows:**
```bash
# Abrir PowerShell como administrador
Get-Service postgresql-x64-*
```

Debe mostrar: `Status: Running`

**En Linux/Mac:**
```bash
pg_isready -h localhost
```

Debe mostrar: `accepting connections`

---

## 🗄️ PASO 2: CREAR BASE DE DATOS EN PostgreSQL

### 2.1 Conectarse a PostgreSQL

```bash
psql -h localhost -U postgres
```

Se pedirá contraseña. Ingrese la contraseña del usuario `postgres`.

### 2.2 Crear base de datos

```sql
-- Crear base de datos
CREATE DATABASE sistema_personas 
  ENCODING 'UTF8' 
  LC_COLLATE 'es_ES.UTF-8' 
  LC_CTYPE 'es_ES.UTF-8';

-- Conectarse a la nueva base de datos
\c sistema_personas

-- Ver bases de datos
\l

-- Salir
\q
```

### 2.3 Crear tabla (OPCIONAL - si aún no existe)

```bash
# Si descargó el archivo sql_script_crear_tabla.sql, ejecute:
psql -h localhost -U postgres -d sistema_personas -f sql_script_crear_tabla.sql
```

O manualmente:

```sql
CREATE TABLE personas (
    id_persona SERIAL PRIMARY KEY,
    nombres VARCHAR(150) NOT NULL,
    apellido_paterno VARCHAR(100) NOT NULL,
    apellido_materno VARCHAR(100) NOT NULL,
    email VARCHAR(150) NOT NULL UNIQUE,
    telefono VARCHAR(20) NOT NULL,
    fecha_nac DATE NOT NULL,
    tipo VARCHAR(20) NOT NULL,
    codigo VARCHAR(50) NOT NULL,
    ci VARCHAR(30) NOT NULL UNIQUE
);
```

---

## 📁 PASO 3: CONFIGURAR PROYECTO ASP.NET CORE

### 3.1 Opción A: Crear proyecto nuevo (recomendado)

```bash
# Crear carpeta del proyecto
mkdir SistemaWeb
cd SistemaWeb

# Crear proyecto MVC
dotnet new mvc -f net8.0

# Crear estructura de carpetas
mkdir Models Data Services

# Copiar archivos del módulo Persona en las carpetas correspondientes
```

### 3.2 Opción B: Usar proyecto existente

Copie los archivos del módulo Persona a su proyecto existente:

```
SistemaWeb/
├── Models/Persona.cs
├── Data/ApplicationDbContext.cs
├── Services/PersonaService.cs
├── Controllers/PersonaController.cs
└── Views/Persona/
    ├── Index.cshtml
    ├── Create.cshtml
    ├── Edit.cshtml
    ├── Details.cshtml
    └── Delete.cshtml
```

---

## 📦 PASO 4: INSTALAR DEPENDENCIAS

### 4.1 Instalar Npgsql.EntityFrameworkCore.PostgreSQL

**Opción 1: Línea de comandos**

```bash
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```

**Opción 2: Package Manager Console (Visual Studio)**

```powershell
Install-Package Npgsql.EntityFrameworkCore.PostgreSQL
```

**Opción 3: NuGet Package Manager (Visual Studio)**
- Clic derecho en proyecto → Manage NuGet Packages
- Buscar: `Npgsql.EntityFrameworkCore.PostgreSQL`
- Instalar la versión 8.0.x

### 4.2 Instalar Microsoft.EntityFrameworkCore.Tools

```bash
dotnet add package Microsoft.EntityFrameworkCore.Tools
```

### 4.3 Instalar dependencias de vistas (Bootstrap, Font Awesome)

**Automático - se incluye en el archivo `_Layout.cshtml`**

O instale manualmente:

```bash
dotnet add package BootstrapJsInterop
```

---

## ⚙️ PASO 5: CONFIGURACIÓN DE CONEXIÓN

### 5.1 Editar appsettings.json

Abra el archivo `appsettings.json` en la raíz del proyecto:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.EntityFrameworkCore": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=sistema_personas;Username=postgres;Password=postgres;Encoding=UTF8"
  }
}
```

**IMPORTANTE:** Reemplace:
- `Database=sistema_personas` → Con el nombre de su base de datos
- `Password=postgres` → Con la contraseña real de PostgreSQL

### 5.2 Parámetros de conexión explicados

| Parámetro | Valor | Descripción |
|-----------|-------|------------|
| `Host` | localhost | Servidor PostgreSQL (local) |
| `Port` | 5432 | Puerto por defecto de PostgreSQL |
| `Database` | sistema_personas | Nombre de la base de datos |
| `Username` | postgres | Usuario de PostgreSQL |
| `Password` | postgres | Contraseña del usuario |
| `Encoding` | UTF8 | Codificación para caracteres especiales |

---

## 📝 PASO 6: CONFIGURAR Program.cs

Reemplace el contenido de `Program.cs` con:

```csharp
using Npgsql;
using SistemaWeb.Data;
using SistemaWeb.Services;

var builder = WebApplicationBuilder.CreateBuilder(args);

// Agregar servicios
builder.Services.AddControllersWithViews();

// Configurar DbContext con PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registrar servicios
builder.Services.AddScoped<IPersonaService, PersonaService>();

// Logging
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
```

---

## 🎨 PASO 7: CONFIGURAR LAYOUT (Bootstrap)

Edite `Views/Shared/_Layout.cshtml`:

```html
<!DOCTYPE html>
<html lang="es">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"] - Sistema Web</title>
    
    <!-- Bootstrap 5 -->
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet">
    <!-- Font Awesome -->
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css">
    
    <style>
        body { background-color: #f8f9fa; }
        .navbar { box-shadow: 0 2px 4px rgba(0,0,0,.1); }
    </style>
</head>
<body>
    <nav class="navbar navbar-expand-lg navbar-dark bg-dark">
        <div class="container">
            <a class="navbar-brand" href="@Url.Action("Index", "Home")">
                <i class="fas fa-home"></i> Sistema Web
            </a>
            <ul class="navbar-nav ms-auto">
                <li class="nav-item">
                    <a class="nav-link" href="@Url.Action("Index", "Persona")">
                        <i class="fas fa-users"></i> Personas
                    </a>
                </li>
            </ul>
        </div>
    </nav>

    <div class="container mt-4">
        @RenderBody()
    </div>

    <footer class="bg-dark text-white text-center py-4 mt-5">
        <p>&copy; 2024 Sistema Web - Gestión de Personas</p>
    </footer>

    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html>
```

---

## 🧪 PASO 8: PRUEBAS DE CONEXIÓN

### 8.1 Compilar proyecto

```bash
dotnet build
```

Si hay errores, verifique:
- Sintaxis de appsettings.json (JSON válido)
- Paquetes NuGet instalados
- Rutas de archivos

### 8.2 Ejecutar aplicación

```bash
dotnet run
```

O en Visual Studio: Presione `F5`

### 8.3 Revisar salida de consola

Debería ver:
```
info: Program[0]
      🚀 Iniciando aplicación ASP.NET Core MVC...
info: Program[0]
      📁 Entorno: Development
info: Microsoft.Hosting.Lifetime[0]
      Now listening on: https://localhost:7000
```

### 8.4 Acceder a la aplicación

Abra el navegador: `https://localhost:7000/Persona`

---

## 🔍 PASO 9: PRIMERA PRUEBA DE FUNCIONALIDAD

### 9.1 Crear una persona

1. Vaya a `/Persona`
2. Haga clic en "Nueva Persona"
3. Complete el formulario:
   - Nombres: `Test User`
   - Apellido Paterno: `Test`
   - Apellido Materno: `Usuario`
   - Email: `test@ejemplo.com`
   - Teléfono: `+591 70000000`
   - Fecha Nacimiento: `2000-01-01`
   - Tipo: `Documento`
   - Código: `TEST001`
   - CI: `1000000`

4. Haga clic en "Guardar Persona"

### 9.2 Verificar en base de datos

```bash
psql -h localhost -U postgres -d sistema_personas

# En PostgreSQL:
SELECT * FROM personas;
```

Debería aparecer el registro creado.

---

## ⚠️ SOLUCIÓN DE PROBLEMAS COMUNES

### Problema: "Connection refused"

```
Npgsql.NpgsqlException: Unable to connect to any of the specified endpoints
```

**Soluciones:**
1. Verifique que PostgreSQL está corriendo
2. Revise host/puerto en appsettings.json
3. Pruebe conexión manual:
   ```bash
   psql -h localhost -U postgres
   ```

### Problema: "Database does not exist"

```
FATAL: database "sistema_personas" does not exist
```

**Solución:**
```bash
createdb -h localhost -U postgres sistema_personas
```

### Problema: "Invalid password"

```
FATAL: password authentication failed for user "postgres"
```

**Solución:**
- Verifique la contraseña en appsettings.json
- Reinicie PostgreSQL
- Cambie contraseña si es necesario

### Problema: "Port 5432 in use"

```
listen tcp 127.0.0.1:5432: bind: address already in use
```

**Solución (Linux/Mac):**
```bash
lsof -i :5432
kill -9 <PID>
```

**Solución (Windows):**
```powershell
netstat -ano | findstr :5432
taskkill /PID <PID> /F
```

### Problema: Vistas no se cargan

**Soluciones:**
1. Limpiar cache: `dotnet clean`
2. Reconstruir: `dotnet build`
3. Verificar que Bootstrap está en `_Layout.cshtml`
4. Reiniciar aplicación

---

## 📊 VERIFICACIÓN FINAL

Ejecute este checklist:

- [ ] PostgreSQL está corriendo
- [ ] Base de datos `sistema_personas` existe
- [ ] Tabla `personas` está creada
- [ ] Paquetes NuGet instalados
- [ ] `appsettings.json` tiene conexión correcta
- [ ] `Program.cs` configurado
- [ ] Vistas contienen Bootstrap
- [ ] Proyecto compila sin errores
- [ ] Aplicación inicia correctamente
- [ ] Puede acceder a `/Persona`
- [ ] Puede crear una persona
- [ ] Registro aparece en base de datos

---

## 🎯 PRÓXIMOS PASOS

1. **Integración:** Agregue el servicio a otros controladores
2. **Personalización:** Ajuste validaciones según necesidades
3. **Pruebas:** Cree pruebas unitarias del servicio
4. **Deployment:** Publique en servidor de producción

---

## 📚 RECURSOS

- [Documentación de .NET 8](https://learn.microsoft.com/es-es/dotnet/core/whats-new/dotnet-8)
- [ASP.NET Core MVC](https://learn.microsoft.com/es-es/aspnet/core/mvc/overview)
- [Entity Framework Core](https://learn.microsoft.com/es-es/ef/core/)
- [Npgsql Documentation](https://www.npgsql.org/doc/)
- [PostgreSQL](https://www.postgresql.org/docs/)
- [Bootstrap 5](https://getbootstrap.com/docs/5.3/)

---

## ✨ ¡Listo para comenzar!

Si todo está configurado correctamente, debería poder:
- ✅ Acceder a la aplicación
- ✅ Ver el listado de personas
- ✅ Crear nuevas personas
- ✅ Editar personas existentes
- ✅ Eliminar personas
- ✅ Buscar y filtrar

**¡Éxito con tu módulo Persona! 🎉**
