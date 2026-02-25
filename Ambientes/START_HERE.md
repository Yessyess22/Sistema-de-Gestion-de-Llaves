# 🚀 START HERE - Comenzar Aquí

¡Bienvenido al proyecto API de Ambientes! Este archivo te guiará de forma rápida para empezar.

---

## ⚡ Inicio Rápido (2 minutos)

### Prerequisito ÚNICO
- **Docker Desktop** instalado (descarga desde https://www.docker.com/products/docker-desktop)

### Ejecutar el Proyecto

```bash
# 1. Navega a la carpeta del proyecto
cd c:\Users\hp\Desktop\Ambientes

# 2. Ejecuta Docker Compose
docker-compose up --build

# 3. ¡Listo! Accede a:
# - API: http://localhost:8080
# - Swagger (Documentación): http://localhost:8080/doc
# - Health Check: http://localhost:8080/health
```

**Eso es todo.** El proyecto está completamente configurado y funcionará de inmediato.

---

## 📚 ¿Qué hace este Proyecto?

Sistema completo de **gestión de ambientes** (aulas, laboratorios, salas de conferencia) con:

- ✅ API REST con 5 endpoints CRUD
- ✅ Base de datos PostgreSQL
- ✅ Arquitectura en capas profesional
- ✅ Docker y Docker Compose incluidos
- ✅ Documentación Swagger interactiva
- ✅ Validaciones robustas
- ✅ Logging y health checks

---

## 📡 Probar la API (Ejemplos Rápidos)

### Opción 1: Usar Swagger (Recomendado - Sin código)
1. Abre: http://localhost:8080/doc
2. Haz clic en "Try it out" en cualquier endpoint
3. Click "Execute"

### Opción 2: Usar cURL (Terminal)

```bash
# Obtener todos los ambientes
curl http://localhost:8080/api/ambientes

# Crear un ambiente
curl -X POST http://localhost:8080/api/ambientes \
  -H "Content-Type: application/json" \
  -d '{
    "codigo":"AULA-001",
    "nombre":"Aula de Clases 1",
    "tipoAmbiente":"Aula",
    "ubicacion":"Edificio A, Piso 1",
    "estado":"Disponible"
  }'

# Obtener por ID
curl http://localhost:8080/api/ambientes/1

# Actualizar
curl -X PUT http://localhost:8080/api/ambientes/1 \
  -H "Content-Type: application/json" \
  -d '{
    "codigo":"AULA-001",
    "nombre":"Aula de Clases 1 - Actualizada",
    "tipoAmbiente":"Aula",
    "ubicacion":"Edificio A, Piso 2",
    "estado":"Ocupado"
  }'

# Eliminar
curl -X DELETE http://localhost:8080/api/ambientes/1
```

### Opción 3: Usar Postman (Requiere instalación)
1. Descargar desde: https://www.postman.com/downloads/
2. Importar archivo: `Postman_Collection.json`
3. Cambiar variable `base_url` a `http://localhost:8080`
4. ¡Listo para probar!

---

## 🏗️ Estructura del Proyecto

```
📁 Ambientes/ (Carpeta Raíz)
├── src/                          # Código fuente
│   ├── Ambientes.API/            # Controladores y configuración
│   ├── Ambientes.Services/       # Lógica de negocio
│   └── Ambientes.Data/           # Modelo y acceso a base de datos
├── docker-compose.yml            # Configuración Docker
├── Dockerfile                    # Construcción de imagen
├── init-db.sql                   # Script de base de datos
├── Ambientes.sln                 # Solución Visual Studio
├── README.md                     # Documentación completa
├── DEVELOPMENT.md                # Guía para desarrollo local
├── PRODUCTION.md                 # Guía para producción
├── QUICK_COMMANDS.md             # Comandos útiles
└── Este archivo
```

---

## 🛑 Detener el Proyecto

Presiona `Ctrl + C` en la terminal donde ejecutaste Docker, o:

```bash
docker-compose down
```

---

## 📖 Documentación Disponible

Según lo que necesites:

| Si quieres... | Lee... |
|---|---|
| **Información completa del proyecto** | [README.md](README.md) |
| **Desarrollar localmente** | [DEVELOPMENT.md](DEVELOPMENT.md) |
| **Desplegar a producción** | [PRODUCTION.md](PRODUCTION.md) |
| **Comandos útiles rápidos** | [QUICK_COMMANDS.md](QUICK_COMMANDS.md) |
| **Resumen de implementación** | [IMPLEMENTATION_SUMMARY.md](IMPLEMENTATION_SUMMARY.md) |

---

## 🆘 Solucionar Problemas

### Puerto 8080 ya está en uso
```bash
# Editar docker-compose.yml línea 35:
ports:
  - "18080:8080"  # Cambiar a otro puerto

# Luego acceder a: http://localhost:18080
```

### PostgreSQL no inicia
```bash
# Eliminar volumen anterior y reintentar
docker-compose down -v
docker-compose up --build
```

### Ver logs de errores
```bash
docker-compose logs api
```

---

## 🎯 Endpoints Principales

| Método | URL | Acción |
|--------|-----|--------|
| GET | `/api/ambientes` | Obtener todos |
| GET | `/api/ambientes/1` | Obtener por ID |
| POST | `/api/ambientes` | Crear nuevo |
| PUT | `/api/ambientes/1` | Actualizar |
| DELETE | `/api/ambientes/1` | Eliminar |

---

## 👨‍💻 Desarrollar Localmente (Opcional)

Si quieres desarrollar sin Docker:

```bash
# Requisitos: .NET 8 SDK y PostgreSQL

cd src/Ambientes.API
dotnet restore
dotnet run

# API en: http://localhost:5000
```

Ver [DEVELOPMENT.md](DEVELOPMENT.md) para detalles completos.

---

## 🚀 Próximos Pasos

1. ✅ **Ejecuta el proyecto**: `docker-compose up --build`
2. ✅ **Accede a Swagger**: http://localhost:8080/doc
3. ✅ **Prueba los endpoints**: Usa Swagger, cURL o Postman
4. ✅ **Lee la documentación**: README.md para más detalles
5. ✅ **Personaliza según necesites**: El código está completamente comentado

---

## 📊 Datos de Prueba

La base de datos incluye estos ambientes de prueba:

```
ID | Código   | Nombre                           | Estado      
---|----------|----------------------------------|----------
1  | LAB-001  | Laboratorio de Informática 1    | Disponible
2  | AULA-101 | Aula de Clases 101              | Disponible
3  | CONF-001 | Sala de Conferencias Principal  | Ocupado
4  | LAB-002  | Laboratorio de Electrónica      | Mantenimiento
```

---

## 🔒 Credenciales (Desarrollo)

Por seguridad, estas credenciales cambian en producción:

```
PostgreSQL:
- Username: admin
- Password: admin123
- Base de datos: ambientes_db
```

⚠️ **IMPORTANTE**: Cambiar en producción. Ver [PRODUCTION.md](PRODUCTION.md)

---

## 📞 Necesitas Ayuda?

- **Errores Docker**: Ver sección "Solucionar Problemas" arriba
- **Documentación completa**: Lee [README.md](README.md)
- **Comandos útiles**: Consulta [QUICK_COMMANDS.md](QUICK_COMMANDS.md)
- **Desarrollo local**: Sigue [DEVELOPMENT.md](DEVELOPMENT.md)

---

## ✨ Características Destacadas

✅ **CRUD Completo**: Crear, leer, actualizar, eliminar ambientes  
✅ **API REST**: Endpoints estándar y bien documentados  
✅ **PostgreSQL**: Base de datos robusta y confiable  
✅ **Swagger**: Documentación interactiva  
✅ **Docker**: Despliegue fácil en cualquier lugar  
✅ **Validaciones**: Datos garantizados consistentes  
✅ **Logging**: Seguimiento de todas las operaciones  
✅ **Health Checks**: Monitoreo de estado  
✅ **Código Profesional**: Comentado, limpio y mantenible  
✅ **Listo para Producción**: Sin configuración adicional  

---

## 🎉 ¡Listo!

Tienes un **proyecto ASP.NET Core production-ready** completo.

Ejecuta: 
```bash
docker-compose up --build
```

Accede a: 
```
http://localhost:8080/doc
```

¡Que disfrutes! 🚀

---

**Version**: 1.0.0  
**Fecha**: Febrero 2024  
**Estado**: ✅ Completado y Listo
