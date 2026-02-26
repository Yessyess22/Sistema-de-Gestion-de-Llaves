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
| PostgreSQL | 15 | Base de datos |
| Docker / Docker Compose | latest | Contenedores |
| BCrypt.Net-Next | 4.0.3 | Hash de contraseñas |
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

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) instalado y en ejecución
- .NET SDK 8.0 (solo si se ejecuta **sin** Docker)

---

### 1. Clonar el repositorio

```bash
git clone <url-del-repo>
cd Sistema-de-Gestion-de-Llaves
```

### 2. Levantar los contenedores

```bash
# Primera vez o después de cambios en el código:
docker-compose up --build

# Ejecución en segundo plano:
docker-compose up --build -d
```

> Docker levanta dos servicios: **`db`** (PostgreSQL 15) y **`webapp`** (ASP.NET Core 8).
> La webapp espera a que la base de datos esté lista antes de arrancar.

La aplicación queda disponible en: **http://localhost:8080**

---

### 3. Migraciones y datos iniciales

Las migraciones y el seeder **se ejecutan automáticamente** al iniciar la webapp. No es necesario ningún comando adicional.

Al arrancar, el sistema realiza en orden:
1. Aplica todas las migraciones pendientes (`db.Database.Migrate()`)
2. Inserta datos de catálogo: tipos de ambiente, roles y permisos
3. Crea el usuario administrador por defecto (si no existe)
4. Inserta ambientes y llaves de ejemplo

Si necesitas ejecutar las migraciones manualmente (fuera de Docker):

```bash
cd src/SistemaGestionLlaves
dotnet ef database update
```

---

### 4. Acceso al sistema

Abre **http://localhost:8080** en el navegador. Serás redirigido automáticamente al Login.

| Campo | Valor |
|---|---|
| **Usuario** | `admin` |
| **Contraseña** | `password` |

> El usuario `admin` tiene rol **Administrador** con acceso total al sistema.

---

### 5. Detener los contenedores

```bash
# Detener sin borrar datos:
docker-compose down

# Detener y borrar la base de datos (reset completo):
docker-compose down -v
```

---

### Solución de problemas comunes

| Síntoma | Causa | Solución |
|---|---|---|
| Error al iniciar la webapp | La BD aún no está lista | Esperar unos segundos; el servicio se reinicia solo (`restart: on-failure`) |
| "relation does not exist" | Migración no aplicada | `docker-compose down -v && docker-compose up --build` |
| "Usuario o contraseña incorrectos" | Hash desactualizado en la BD | Reiniciar con `docker-compose up --build`; el seeder actualiza el hash automáticamente |
| Puerto 8080 ocupado | Otro proceso usa el puerto | Cambiar el puerto en `docker-compose.yml` → `"8081:8080"` |

---

## 🗄️ Base de Datos

La base de datos **PostgreSQL 15** se inicializa automáticamente al levantar Docker. Las migraciones se aplican al iniciar la aplicación vía EF Core.

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
