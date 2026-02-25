# API de Ambientes - Sistema de Gestión

Proyecto ASP.NET Core Web API para la gestión de ambientes con arquitectura en capas, utilizando PostgreSQL, Docker y Docker Compose.

## 📋 Características

- ✅ **CRUD Completo** para la entidad Ambiente
- ✅ **Arquitectura en Capas**: API, Services, Data
- ✅ **Entity Framework Core 8** con PostgreSQL
- ✅ **Validaciones** con Data Annotations
- ✅ **RESTful API** con documentación Swagger
- ✅ **Docker y Docker Compose** para contenerización
- ✅ **Logging** con Serilog
- ✅ **Health Checks** integrados
- ✅ **Manejo de Errores** robusto
- ✅ **Código comentado** profesional
- ✅ **Listo para Producción**

---

## 🏗️ Estructura del Proyecto

```
Ambientes/
├── src/
│   ├── Ambientes.API/                    # Capa de Presentación (Web API)
│   │   ├── Controllers/
│   │   │   └── AmbientesController.cs    # Controlador REST
│   │   ├── Program.cs                    # Configuración de la aplicación
│   │   ├── appsettings.json              # Configuración producción
│   │   ├── appsettings.Development.json  # Configuración desarrollo
│   │   └── Ambientes.API.csproj
│   │
│   ├── Ambientes.Services/               # Capa de Servicios (Lógica de Negocio)
│   │   ├── Interfaces/
│   │   │   └── IAmbienteService.cs       # Contrato del servicio
│   │   ├── Implementations/
│   │   │   └── AmbienteService.cs        # Implementación del servicio
│   │   └── Ambientes.Services.csproj
│   │
│   └── Ambientes.Data/                   # Capa de Datos
│       ├── Models/
│       │   └── Ambiente.cs               # Modelo de datos
│       ├── Context/
│       │   └── AmbientesDbContext.cs     # DbContext de EF Core
│       ├── Repositories/
│       │   └── AmbienteRepository.cs     # Repositorio de acceso a datos
│       └── Ambientes.Data.csproj
│
├── Dockerfile                             # Construcción de imagen Docker
├── docker-compose.yml                     # Orquestación de contenedores
├── init-db.sql                            # Script de inicialización de PostgreSQL
├── Ambientes.sln                          # Solución Visual Studio
├── README.md                              # Este archivo
└── .gitignore                             # Archivos ignorados por Git

```

---

## 🚀 Requisitos Previos

### Opción 1: Con Docker (Recomendado)
- **Docker Desktop** (Windows, macOS) o **Docker Engine** (Linux)
- **Docker Compose** (incluida con Docker Desktop)

### Opción 2: Desarrollo Local
- **.NET SDK 8.0** o superior
- **PostgreSQL 14+** instalado localmente
- **Visual Studio 2022** o **Visual Studio Code**
- **Git** (opcional pero recomendado)

---

## 📦 Instalación y Ejecución

### 🐳 Opción 1: Ejecutar con Docker Compose (RECOMENDADO)

#### Paso 1: Clonar el repositorio
```bash
git clone https://github.com/usuario/Ambientes.git
cd Ambientes
```

#### Paso 2: Ejecutar Docker Compose
```bash
docker-compose up --build
```

Este comando:
- Construye la imagen Docker de la API
- Crea un contenedor PostgreSQL
- Ejecuta el script `init-db.sql` para inicializar la BD
- Inicia ambos servicios
- Aplica las migraciones automáticamente

#### Paso 3: Verificar que está funcionando
- **API**: http://localhost:8080
- **Swagger**: http://localhost:8080/doc
- **Health Check**: http://localhost:8080/health

#### Paso 4: Detener los servicios
```bash
docker-compose down
```

Para ver los logs en tiempo real:
```bash
docker-compose logs -f api
```

---

### 💻 Opción 2: Ejecutar Localmente (Desarrollo)

#### Paso 1: Instalar .NET 8
Descargar desde: https://dotnet.microsoft.com/download/dotnet/8.0

#### Paso 2: Configurar PostgreSQL
```bash
# Windows - Instalar con Chocolatey
choco install postgresql

# macOS - Instalar con Homebrew
brew install postgresql

# Linux - Usar el gestor de paquetes
sudo apt-get install postgresql postgresql-contrib
```

Crear base de datos:
```bash
createdb -U postgres ambientes_db_dev

# Ejecutar el script de inicialización
psql -U postgres -d ambientes_db_dev -f init-db.sql
```

#### Paso 3: Actualizar la cadena de conexión
Editar `src/Ambientes.API/appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "PostgreSQL": "Host=localhost;Port=5432;Database=ambientes_db_dev;Username=postgres;Password=tu_password;Ssl Mode=Disable;"
  }
}
```

#### Paso 4: Restaurar dependencias
```bash
cd src/Ambientes.API
dotnet restore
```

#### Paso 5: Ejecutar la aplicación
```bash
dotnet run
```

La API estará disponible en: http://localhost:5000

Swagger estará en: http://localhost:5000/doc

---

## 📡 Endpoints de la API

### Base URL
```
http://localhost:8080/api/ambientes
```

### 1. Obtener todos los ambientes
```http
GET /api/ambientes
```

**Respuesta exitosa (200):**
```json
[
  {
    "id": 1,
    "codigo": "LAB-001",
    "nombre": "Laboratorio de Informática 1",
    "tipoAmbiente": "Laboratorio",
    "ubicacion": "Edificio A, Piso 2",
    "estado": "Disponible",
    "fechaCreacion": "2024-02-24T10:30:00Z",
    "fechaActualizacion": "2024-02-24T10:30:00Z"
  }
]
```

---

### 2. Obtener ambiente por ID
```http
GET /api/ambientes/{id}
```

**Ejemplo:**
```bash
curl -X GET http://localhost:8080/api/ambientes/1
```

**Respuesta exitosa (200):**
```json
{
  "id": 1,
  "codigo": "LAB-001",
  "nombre": "Laboratorio de Informática 1",
  "tipoAmbiente": "Laboratorio",
  "ubicacion": "Edificio A, Piso 2",
  "estado": "Disponible",
  "fechaCreacion": "2024-02-24T10:30:00Z",
  "fechaActualizacion": "2024-02-24T10:30:00Z"
}
```

**Errores:**
- **400**: ID inválido (menor o igual a 0)
- **404**: Ambiente no encontrado

---

### 3. Crear nuevo ambiente
```http
POST /api/ambientes
Content-Type: application/json

{
  "codigo": "AULA-102",
  "nombre": "Aula de Clases 102",
  "tipoAmbiente": "Aula",
  "ubicacion": "Edificio B, Piso 1",
  "estado": "Disponible"
}
```

**Ejemplo con curl:**
```bash
curl -X POST http://localhost:8080/api/ambientes \
  -H "Content-Type: application/json" \
  -d '{
    "codigo": "AULA-102",
    "nombre": "Aula de Clases 102",
    "tipoAmbiente": "Aula",
    "ubicacion": "Edificio B, Piso 1",
    "estado": "Disponible"
  }'
```

**Respuesta exitosa (201):**
```json
{
  "id": 5,
  "codigo": "AULA-102",
  "nombre": "Aula de Clases 102",
  "tipoAmbiente": "Aula",
  "ubicacion": "Edificio B, Piso 1",
  "estado": "Disponible",
  "fechaCreacion": "2024-02-24T15:45:00Z",
  "fechaActualizacion": "2024-02-24T15:45:00Z"
}
```

**Errores:**
- **400**: Datos inválidos o incompletos
- **409**: Código duplicado (ya existe)

---

### 4. Actualizar ambiente
```http
PUT /api/ambientes/{id}
Content-Type: application/json

{
  "codigo": "LAB-001",
  "nombre": "Laboratorio de Informática 1 - Actualizado",
  "tipoAmbiente": "Laboratorio",
  "ubicacion": "Edificio A, Piso 2",
  "estado": "Mantenimiento"
}
```

**Ejemplo con curl:**
```bash
curl -X PUT http://localhost:8080/api/ambientes/1 \
  -H "Content-Type: application/json" \
  -d '{
    "codigo": "LAB-001",
    "nombre": "Laboratorio de Informática 1 - Actualizado",
    "tipoAmbiente": "Laboratorio",
    "ubicacion": "Edificio A, Piso 2",
    "estado": "Mantenimiento"
  }'
```

**Respuesta exitosa (200):**
```json
{
  "id": 1,
  "codigo": "LAB-001",
  "nombre": "Laboratorio de Informática 1 - Actualizado",
  "tipoAmbiente": "Laboratorio",
  "ubicacion": "Edificio A, Piso 2",
  "estado": "Mantenimiento",
  "fechaCreacion": "2024-02-24T10:30:00Z",
  "fechaActualizacion": "2024-02-24T16:00:00Z"
}
```

**Errores:**
- **400**: ID inválido o datos inválidos
- **404**: Ambiente no encontrado
- **409**: Código duplicado

---

### 5. Eliminar ambiente
```http
DELETE /api/ambientes/{id}
```

**Ejemplo con curl:**
```bash
curl -X DELETE http://localhost:8080/api/ambientes/1
```

**Respuesta exitosa (204):**
Sin contenido (No Content)

**Errores:**
- **400**: ID inválido
- **404**: Ambiente no encontrado

---

## 📊 Estados Válidos de Ambiente

Los ambientes solo pueden tener los siguientes estados:

| Estado | Descripción |
|--------|-------------|
| `Disponible` | El ambiente está disponible para usar |
| `Ocupado` | El ambiente está siendo utilizado |
| `Mantenimiento` | El ambiente está en mantenimiento |

---

## 🗄️ Estructura de la Base de Datos

### Tabla: ambientes

```sql
CREATE TABLE ambientes (
    id SERIAL PRIMARY KEY,
    codigo VARCHAR(50) NOT NULL UNIQUE,           -- Código único
    nombre VARCHAR(100) NOT NULL,                 -- Nombre del ambiente
    tipo_ambiente VARCHAR(50) NOT NULL,           -- Tipo
    ubicacion VARCHAR(100) NOT NULL,              -- Ubicación
    estado VARCHAR(30) NOT NULL,                  -- Estado
    fecha_creacion TIMESTAMP DEFAULT NOW(),       -- Fecha de creación
    fecha_actualizacion TIMESTAMP DEFAULT NOW()   -- Fecha de actualización
);
```

### Índices
- `idx_ambiente_codigo_unique`: Para búsquedas rápidas por código
- `idx_ambiente_estado`: Para filtrar por estado
- `idx_ambiente_nombre`: Para búsquedas por nombre

---

## 📝 Validaciones

### Validaciones en el Modelo

| Campo | Validación |
|-------|-----------|
| `Codigo` | Requerido, máximo 50 caracteres, único |
| `Nombre` | Requerido, máximo 100 caracteres |
| `TipoAmbiente` | Requerido, máximo 50 caracteres |
| `Ubicacion` | Requerido, máximo 100 caracteres |
| `Estado` | Requerido, debe ser: Disponible, Ocupado o Mantenimiento |

### Errores de Validación

Cuando hay errores de validación, se retorna un **400 Bad Request** con formato:

```json
{
  "mensaje": "Validación fallida",
  "errores": {
    "codigo": ["El código del ambiente es requerido"],
    "estado": ["El estado debe ser uno de: Disponible, Ocupado, Mantenimiento"]
  }
}
```

---

## 🔧 Configuración de Appsettings

### appsettings.json (Producción)
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "ConnectionStrings": {
    "PostgreSQL": "Host=postgres;Port=5432;Database=ambientes_db;Username=admin;Password=admin123;Ssl Mode=Disable;"
  },
  "AllowedHosts": "*"
}
```

### appsettings.Development.json (Desarrollo)
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "ConnectionStrings": {
    "PostgreSQL": "Host=localhost;Port=5432;Database=ambientes_db_dev;Username=admin;Password=admin123;Ssl Mode=Disable;"
  }
}
```

---

## 🐳 Comandos Docker Útiles

### Iniciar servicios
```bash
docker-compose up --build
```

### Iniciar en segundo plano
```bash
docker-compose up -d --build
```

### Ver logs en tiempo real
```bash
docker-compose logs -f
docker-compose logs -f api
docker-compose logs -f postgres
```

### Detener servicios
```bash
docker-compose down
```

### Detener y eliminar volúmenes (incluida la BD)
```bash
docker-compose down -v
```

### Reconstruir imágenes
```bash
docker-compose build --no-cache
```

### Acceder al contenedor de la API
```bash
docker exec -it ambientes-api /bin/bash
```

### Acceder a PostgreSQL
```bash
docker exec -it ambientes-postgres psql -U admin -d ambientes_db
```

---

## 🧪 Testing Manual con Postman o cURL

### Coleción de Postman
Se puede crear fácilmente importando desde Swagger en:
```
http://localhost:8080/swagger/v1/swagger.json
```

### Ejemplos con cURL

#### Crear ambiente
```bash
curl -i -X POST http://localhost:8080/api/ambientes \
  -H "Content-Type: application/json" \
  -d '{"codigo":"TEST-001","nombre":"Aula Test","tipoAmbiente":"Aula","ubicacion":"Piso 1","estado":"Disponible"}'
```

#### Obtener todos
```bash
curl -i http://localhost:8080/api/ambientes
```

#### Obtener por ID
```bash
curl -i http://localhost:8080/api/ambientes/1
```

#### Actualizar
```bash
curl -i -X PUT http://localhost:8080/api/ambientes/1 \
  -H "Content-Type: application/json" \
  -d '{"codigo":"TEST-001","nombre":"Aula Test Updated","tipoAmbiente":"Aula","ubicacion":"Piso 2","estado":"Ocupado"}'
```

#### Eliminar
```bash
curl -i -X DELETE http://localhost:8080/api/ambientes/1
```

---

## 📊 Health Check

Verificar el estado de la aplicación:
```bash
curl http://localhost:8080/health
```

**Respuesta (200):**
```json
{
  "status": "Healthy"
}
```

---

## 📚 Documentación API (Swagger)

La documentación interactiva está disponible en:
```
http://localhost:8080/doc
```

**Características:**
- ✅ Documentación de todos los endpoints
- ✅ Pruebas directas desde el navegador
- ✅ Esquemas de datos
- ✅ Códigos de respuesta HTTP

---

## 🏭 Pipeline de CI/CD (Ejemplo para GitHub Actions)

Crear archivo `.github/workflows/docker-publish.yml`:

```yaml
name: Docker Build and Publish

on:
  push:
    branches: [ main ]

jobs:
  build:
    runs-on: ubuntu-latest
    
    steps:
      - uses: actions/checkout@v3
      - name: Build and push Docker image
        run: docker-compose build
```

---

## 🔍 Troubleshooting

### Error: "Connection refused" en PostgreSQL
**Solución:** Asegúrate de que PostgreSQL está corriendo y espera a que el contenedor esté listo:
```bash
docker-compose down -v
docker-compose up --build
```

### Error: "Port already in use"
**Solución:** Cambiar puertos en `docker-compose.yml`:
```yaml
ports:
  - "18080:8080"  # API en puerto 18080
  - "15432:5432"  # PostgreSQL en puerto 15432
```

### Error: "Database does not exist"
**Solución:** Ejecutar el script SQL manualmente:
```bash
docker exec ambientes-postgres psql -U admin -d postgres -f /docker-entrypoint-initdb.d/init.sql
```

### Logs de error en la API
```bash
docker-compose logs api | tail -50
```

---

## 🎯 Próximas Mejoras Potenciales

- [ ] Agregación: Paginación en endpoints GET
- [ ] Autenticación: JWT o Identity
- [ ] Caché: Redis para datos frecuentes
- [ ] Unit Tests: xUnit o NUnit
- [ ] Integration Tests
- [ ] API Versioning (v1, v2)
- [ ] Rate Limiting
- [ ] Swagger Versioning
- [ ] Integración con Elasticsearch para búsquedas avanzadas
- [ ] Auditoría: Tabla de cambios en ambientes

---

## 📄 Licencia

MIT License - Libre para usar en proyectos comerciales y personales

---

## 👨‍💻 Contribuciones

Las contribuciones son bienvenidas. Por favor:

1. Fork el repositorio
2. Crear una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

---

## 📞 Contacto y Soporte

Para reportar bugs o solicitar features:
- GitHub Issues: https://github.com/usuario/Ambientes/issues
- Email: desarrollo@ejemplo.com

---

## 📌 Notas Finales

**Este proyecto está listo para producción:**
- ✅ Arquitectura en capas escalable
- ✅ Validaciones robustas
- ✅ Manejo de errores completo
- ✅ Logging con Serilog
- ✅ Docker para fácil despliegue
- ✅ Documentación detallada
- ✅ Health checks integrados

**Recomendaciones al pasar a producción:**
1. Cambiar credenciales de PostgreSQL en `.env`
2. Habilitar HTTPS en la API
3. Configurar CORS según necesidades
4. Implementar autenticación/autorización
5. Configurar backup de base de datos
6. Monitoreo con Prometheus/Grafana
7. Logs centralizados (ELK Stack)

---

**Version**: 1.0.0
**Última actualización**: Febrero 2024
**Autor**: Tu Nombre / Equipo de Desarrollo
