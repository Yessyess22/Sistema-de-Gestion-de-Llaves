# 🔑 Sistema de Gestión de Llaves

**Universidad Privada Domingo Savio (UPDS)**  
**Materia:** Desarrollo de Sistemas II  
**Sprint:** 1  
**Rama:** `quinteros-ramirez-jose-denis`

---

## 📋 Descripción

Sistema web para la gestión, préstamo y control de llaves de ambientes universitarios. Permite administrar quién tiene acceso a qué llave, registrar préstamos y devoluciones, gestionar reservas, y auditar todas las operaciones.

## 🛠️ Stack Tecnológico

| Tecnología | Versión | Uso |
|---|---|---|
| ASP.NET Core MVC | 8.0 | Framework principal |
| Entity Framework Core | 8.0 | ORM / Migraciones |
| SQL Server | 2022 | Base de datos |
| Docker / Docker Compose | latest | Contenedores |
| C# | 12 | Lenguaje de programación |

## 📁 Estructura del Proyecto

```
sistema-gestion-llaves/
├── src/
│   └── SistemaGestionLlaves/
│       ├── Controllers/          # Controladores MVC
│       ├── Models/               # Modelos de dominio (entidades)
│       ├── Data/
│       │   ├── ApplicationDbContext.cs
│       │   └── Migrations/       # Migraciones EF Core
│       ├── Views/                # Vistas Razor
│       ├── wwwroot/              # Archivos estáticos (CSS, JS)
│       ├── appsettings.json
│       └── Program.cs
├── docs/
│   └── DIAGRAMA_ER.md           # Diagrama Entidad-Relación
├── scripts/
│   └── init.sql                 # Script SQL inicial
├── docker-compose.yml
├── Dockerfile
├── .gitignore
└── README.md
```

## 🚀 Inicio Rápido

### Prerrequisitos

- Docker Desktop instalado y ejecutándose
- .NET SDK 8.0 (solo si se ejecuta sin Docker)

### Levantar con Docker Compose

```bash
# Clonar el repositorio
git clone <url-del-repo>
cd sistema-gestion-llaves

# Construir e iniciar todos los contenedores
docker-compose up --build

# En segundo plano:
docker-compose up --build -d
```

La aplicación estará disponible en: **<http://localhost:8080>**

### Credenciales por defecto

| Campo | Valor |
|---|---|
| Usuario | `admin` |
| Contraseña | `Admin@1234` |

### Detener los contenedores

```bash
docker-compose down

# Eliminar también los volúmenes (borra la BD)
docker-compose down -v
```

## 🗄️ Base de Datos

La base de datos se inicializa automáticamente al levantar Docker. Las migraciones se aplican al iniciar la aplicación.

### Tablas principales

- **Persona** - Personas registradas en el sistema
- **Rol / Permisos** - Control de acceso basado en roles
- **Usuario** - Cuentas de acceso al sistema
- **Ambiente / TipoAmbiente** - Ambientes físicos y su clasificación
- **Llave** - Llaves de los ambientes
- **Prestamo** - Préstamos de llaves
- **Reserva** - Reservas anticipadas
- **Auditoria** - Trazabilidad de operaciones

Ver el [Diagrama ER completo](docs/DIAGRAMA_ER.md).

## 👥 Equipo de Desarrollo

| Integrante | Rol en el equipo |
|---|---|
| Jose Denis Quinteros Ramírez | Base de datos |

## 📌 Notas del Sprint 1

- Soft delete implementado (campo `estado`) en lugar de eliminación física
- Contraseñas almacenadas como hash (BCrypt)
- Nomenclatura en español para tablas y columnas
- Base de datos en 3FN (Tercera Forma Normal)
