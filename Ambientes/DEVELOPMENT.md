# 🚀 Guía de Desarrollo Local

Esta guía te ayudará a configurar el proyecto para desarrollo local en tu máquina.

## ✅ Requisitos Previos

1. **Visual Studio 2022** o **Visual Studio Code**
2. **.NET SDK 8.0** - Descargar desde: https://dotnet.microsoft.com/download/dotnet/8.0
3. **PostgreSQL 14+** - Descargar desde: https://www.postgresql.org/download/
4. **Git** (opcional pero recomendado)

---

## 🔧 Instalación Paso a Paso

### Paso 1: Instalar PostgreSQL

#### En Windows
1. Descargar desde: https://www.postgresql.org/download/windows/
2. Ejecutar el instalador
3. Recordar la contraseña del usuario `postgres`
4. Dejar el puerto por defecto (5432)

#### En macOS
```bash
brew install postgresql@14
brew services start postgresql@14
```

#### En Linux (Ubuntu/Debian)
```bash
sudo apt-get update
sudo apt-get install postgresql postgresql-contrib
sudo service postgresql start
```

### Paso 2: Crear la base de datos

Abrir pgAdmin o terminal:

```bash
# Conectar a PostgreSQL
psql -U postgres

# Dentro de psql
CREATE DATABASE ambientes_db_dev;
```

O usando pgAdmin:
1. Clic derecho en "Databases"
2. Crear nueva BD: `ambientes_db_dev`

### Paso 3: Clonar el repositorio

```bash
git clone https://github.com/usuario/Ambientes.git
cd Ambientes
```

### Paso 4: Restaurar paquetes NuGet

#### Con Visual Studio 2022
1. Abrir `Ambientes.sln`
2. Menú: Build → Rebuild Solution

#### Con línea de comandos
```bash
dotnet restore
```

### Paso 5: Ejecutar migraciones de EF Core

```bash
# Navegar a la carpeta de la API
cd src/Ambientes.API

# Aplicar migraciones
dotnet ef database update --project ../Ambientes.Data/Ambientes.Data.csproj
```

Si aún no hay migraciones, crear la inicial:
```bash
dotnet ef migrations add InitialCreate --project ../Ambientes.Data/Ambientes.Data.csproj
dotnet ef database update --project ../Ambientes.Data/Ambientes.Data.csproj
```

---

## 🚀 Ejecutar la Aplicación

### Con Visual Studio 2022
1. Abrir `Ambientes.sln`
2. Click derecho en `Ambientes.API`
3. Set as Startup Project
4. Presionar `F5` o click en "Start/Run"

### Con Visual Studio Code
```bash
cd src/Ambientes.API
code .
```

Luego crear `.vscode/launch.json`:
```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": ".NET Core Launch (web)",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${workspaceFolder}/bin/Debug/net8.0/Ambientes.API.dll",
      "args": [],
      "cwd": "${workspaceFolder}",
      "stopAtEntry": false,
      "serverReadyAction": {
        "pattern": "\\bNow listening on:\\s+(https?://\\S+)",
        "uriFormat": "{0}",
        "action": "openExternally"
      }
    }
  ]
}
```

Presionar `F5` para ejecutar.

### Con línea de comandos
```bash
cd src/Ambientes.API
dotnet run
```

La API estará disponible en: **http://localhost:5000**

---

## 🌐 Acceder a la Aplicación

Una vez ejecutada:

1. **Swagger UI**: http://localhost:5000/doc
2. **Home**: http://localhost:5000/
3. **Health Check**: http://localhost:5000/health

---

## 📝 Estructura de carpetas para desarrollo

```
Ambientes/
├── src/
│   ├── Ambientes.API/      ← Abrir aquí con VS Code
│   ├── Ambientes.Services/
│   └── Ambientes.Data/
├── .vscode/                ← Configuración VS Code
│   ├── launch.json
│   └── settings.json
└── Ambientes.sln          ← Abrir aquí con Visual Studio
```

---

## 🔍 Debugging

### Breakpoints en Visual Studio
1. Click en el número de línea donde quieras un breakpoint
2. Ejecutar con F5
3. El código se pausará en el breakpoint

### Debugging en VS Code
1. Presiona F5
2. Elige ".NET Core" como ambiente
3. Los breakpoints se activarán automáticamente

### Ver logs en tiempo real
```bash
dotnet run --verbosity Debug
```

---

## 🧪 Testing Manual

### Usando Postman
1. Importar `Postman_Collection.json`
2. En variables, cambiar `base_url` a `http://localhost:5000`

### Usando cURL
```bash
# Crear ambiente
curl -X POST http://localhost:5000/api/ambientes \
  -H "Content-Type: application/json" \
  -d '{"codigo":"DEV-001","nombre":"Aula de Desarrollo","tipoAmbiente":"Aula","ubicacion":"Casa","estado":"Disponible"}'

# Obtener todos
curl http://localhost:5000/api/ambientes

# Obtener por ID
curl http://localhost:5000/api/ambientes/1
```

---

## 📊 Base de Datos

### Conectarse a PostgreSQL
```bash
# Línea de comandos
psql -U postgres -d ambientes_db_dev

# Dentro de psql
SELECT * FROM ambientes;
```

### Con pgAdmin
1. Abrir pgAdmin (http://localhost:5050)
2. Conectar a servidor local
3. Navegar a: ambientes_db_dev → Schemas → public → Tables → ambientes

---

## 🔌 Variables de Entorno

Editar `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "ConnectionStrings": {
    "PostgreSQL": "Host=localhost;Port=5432;Database=ambientes_db_dev;Username=postgres;Password=TU_PASSWORD;Ssl Mode=Disable;"
  }
}
```

---

## 🆘 Troubleshooting

### Error: "Connection refused" en PostgreSQL
```bash
# Verificar que PostgreSQL está corriendo
sudo service postgresql status

# En Windows, abrir Services y buscar PostgreSQL
```

### Error: "Database does not exist"
```bash
# Crear la base de datos manualmente
psql -U postgres -c "CREATE DATABASE ambientes_db_dev;"
```

### Error en EF Core migrations
```bash
# Limpiar y reconstruir
dotnet clean
dotnet restore
dotnet ef database drop
dotnet ef database update
```

### Puerto 5000 ya está en uso
Editar `Properties/launchSettings.json`:
```json
"applicationUrl": "http://localhost:5001"
```

---

## 📦 Comandos Útiles

```bash
# Restaurar dependencias
dotnet restore

# Compilar
dotnet build

# Ejecutar tests
dotnet test

# Limpiar
dotnet clean

# Ver versión de .NET
dotnet --version

# Crear migración
dotnet ef migrations add NombreMigracion --project ../Ambientes.Data

# Aplicar migraciones
dotnet ef database update --project ../Ambientes.Data

# Revertir última migración
dotnet ef database update nombre_migracion_anterior --project ../Ambientes.Data

# Listar migraciones
dotnet ef migrations list --project ../Ambientes.Data
```

---

## 🎯 Flujo de desarrollo

1. Crear rama para tu feature: `git checkout -b feature/mi-feature`
2. Hacer cambios en el código
3. Ejecutar la aplicación y probar
4. Ver logs para bugs
5. Commit: `git commit -m 'Agregar mi feature'`
6. Push: `git push origin feature/mi-feature`
7. Crear Pull Request

---

## 📚 Recursos útiles

- [Documentación .NET 8](https://docs.microsoft.com/dotnet/core/)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [ASP.NET Core](https://docs.microsoft.com/aspnet/core/)

---

**¡Desarrollo feliz! 🚀**
