# Módulo PERSONA - ASP.NET Core API

API para la gestión de Personas en ASP.NET Core 8, con arquitectura en capas, Entity Framework Core y PostgreSQL. El proyecto fue originalmente MVC y se adaptó a una API REST.

---

## 📋 Tabla de Contenidos

1. [Requisitos Previos](#requisitos-previos)
2. [Estructura del Proyecto](#estructura-del-proyecto)
3. [Instalación de Dependencias](#instalación-de-dependencias)
4. [Configuración](#configuración)
5. [Ejecución](#ejecución)
6. [API REST](#api-rest)
7. [Pruebas](#pruebas)
8. [Resolución de Problemas](#resolución-de-problemas)

---

## 🔧 Requisitos Previos

- **.NET 8 SDK** - https://dotnet.microsoft.com/download/dotnet/8.0
- **Docker & Docker Compose** (recomendado)
- **PostgreSQL** (si no usa la instancia en Docker)
- **Visual Studio 2022** o **Visual Studio Code**
- **Git** (opcional)

---

## 📁 Estructura del Proyecto (relevante)

```
SistemaWeb/
├── Models/
│   └── Persona.cs
├── Data/
│   └── ApplicationDbContext.cs
├── Services/
│   └── PersonaService.cs
├── Controllers/
│   └── PersonaController.cs  # Ahora ApiController
├── Views/                    # Opcional (proyecto migrado a API)
├── appsettings.json
├── Program.cs
└── README.md
```

---

## 📦 Instalación de Dependencias

Instale paquetes necesarios (si hace cambios locales):

```bash
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.EntityFrameworkCore.Tools
```

---

## ⚙️ Configuración

### Cadena de conexión

La aplicación usa `ConnectionStrings:DefaultConnection`. Con `docker-compose` la cadena se configura automáticamente para apuntar al servicio `db`.

Ejemplo en `appsettings.json` (solo para ejecución local):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=nombre_base_datos;Username=postgres;Password=tu_contraseña;Encoding=UTF8"
  }
}
```

---

## 🚀 Ejecución

1) Usar Docker Compose (recomendado):

```bash
docker-compose up --build
```

- API: http://localhost:8000
- Swagger (si está en `Development`): http://localhost:8000/swagger
- Endpoint base: `http://localhost:8000/api/persona`

2) Ejecutar local con `dotnet` (requiere Postgres local o cadena de conexión ajustada):

```bash
dotnet run --project SistemaWeb.csproj
```

Si ejecuta la imagen por separado, exporte la variable `ConnectionStrings__DefaultConnection` para apuntar a su base de datos.

---

## 📡 API REST

Base: `/api/persona`

- GET `/api/persona` — Listar (opcional `?busqueda=...`)
- GET `/api/persona/{id}` — Obtener por id
- POST `/api/persona` — Crear
- PUT `/api/persona/{id}` — Actualizar
- DELETE `/api/persona/{id}` — Eliminar
- GET `/api/persona/PorTipo?tipo=Documento` — Filtrar por tipo

Los endpoints retornan JSON y códigos HTTP estándar (200, 201, 204, 400, 404, 500).

---

## 🧪 Pruebas (rápidas)

1) Con Docker Compose levantado:

```bash
docker-compose up --build
curl http://localhost:8000/api/persona
```

2) Crear (ejemplo):

```bash
curl -X POST http://localhost:8000/api/persona \
  -H "Content-Type: application/json" \
  -d '{"Nombres":"Juan","ApellidoPaterno":"García","ApellidoMaterno":"Pérez","Email":"juan@ejemplo.com","Telefono":"+59112345678","FechaNac":"1990-05-15","Tipo":"Documento","Codigo":"P001","CI":"1234567"}'
```

3) Swagger UI (si está activo): http://localhost:8000/swagger

---

## 🔍 Resolución de Problemas

- "No se puede conectar a PostgreSQL": verifique la cadena de conexión o inicie Postgres. Si usa `docker-compose`, el servicio `db` estará disponible.
- "Error 500": revise los logs del contenedor `web` o la salida de `dotnet run`.

Comandos útiles:

```bash
docker-compose logs -f web
docker exec -it <container_id> /bin/sh
```

---

Si prefieres, puedo eliminar o archivar la carpeta `Views/` ahora que la aplicación es API. ¿Lo hago?

### Problema 5: "Routing no funciona"

**Solución:**
1. Verifique que el controlador es `PersonaController`
2. La URL debe ser `/Persona/...` (capitalize la primera letra)
3. Revise la configuración en `Program.cs`

---

## 📊 Propiedades Calculadas del Modelo

### NombreCompleto
Concatena nombres y apellidos automáticamente:
```csharp
var nombreCompleto = persona.NombreCompleto; // "Juan Carlos García López"
```

### Edad
Calcula la edad actual basada en la fecha de nacimiento:
```csharp
var edad = persona.Edad; // 34
```

---

## 🔐 Validaciones Implementadas

| Campo | Validación |
|-------|-----------|
| Nombres | Requerido, 2-150 caracteres |
| Apellido Paterno | Requerido, 2-100 caracteres |
| Apellido Materno | Requerido, 2-100 caracteres |
| Email | Requerido, formato válido, único |
| Teléfono | Requerido, 7-20 caracteres, solo números/símbolos |
| Fecha Nacimiento | Requerido, mayor de 18 años |
| Tipo | Requerido, "Documento" o "Empresa" |
| Código | Requerido, máximo 50 caracteres |
| CI | Requerido, máximo 30 caracteres, único |

---

## 📚 Archivos Clave

| Archivo | Propósito |
|---------|----------|
| [Persona.cs](Models/Persona.cs) | Modelo con validaciones |
| [ApplicationDbContext.cs](Data/ApplicationDbContext.cs) | Contexto EF Core |
| [PersonaService.cs](Services/PersonaService.cs) | Lógica de negocio |
| [PersonaController.cs](Controllers/PersonaController.cs) | Controlador CRUD |
| [Index.cshtml](Views/Persona/Index.cshtml) | Listado |
| [Create.cshtml](Views/Persona/Create.cshtml) | Crear |
| [Edit.cshtml](Views/Persona/Edit.cshtml) | Editar |
| [Details.cshtml](Views/Persona/Details.cshtml) | Detalles |
| [Delete.cshtml](Views/Persona/Delete.cshtml) | Eliminar |

---

## 🎯 Próximos Pasos

1. **Integración con otros módulos:** El servicio está listo para inyectarse en otros controladores
2. **Reportes:** Crear vistas para exportar a Excel/PDF
3. **Auditoría:** Agregar campos de auditoría (CreatedAt, UpdatedAt, CreatedBy)
4. **Paginación:** Implementar paginación en listados grandes
5. **API REST:** Crear endpoints API para consumo desde frontend

---

## 📝 Notas Importantes

- ✅ **Sin migraciones:** El módulo asume que la tabla ya existe en PostgreSQL
- ✅ **Validaciones en cliente y servidor:** Mejor experiencia de usuario
- ✅ **Inyección de dependencias:** Fácil de testear
- ✅ **Logging:** Todos los eventos principales se registran
- ✅ **Manejo de errores:** Mensajes claros al usuario

---

## 👨‍💻 Autor

Desarrollado siguiendo estándares profesionales de ASP.NET Core y arquitectura en capas.

**Versión:** 1.0  
**Fecha:** Febrero 2024  
**Framework:** .NET 8 + ASP.NET Core MVC 8

---

## 📞 Soporte

Si encuentra problemas:
1. Revise este README
2. Verifique los logs en la consola
3. Consulte la sección de "Resolución de Problemas"
4. Verifique la conectividad con PostgreSQL

---

**¡Listo para usar!** ✨
