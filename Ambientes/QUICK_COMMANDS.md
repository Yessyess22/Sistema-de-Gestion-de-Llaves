# ⚡ Comandos Rápidos

Referencia rápida de comandos más usados durante desarrollo.

## 🐳 Docker

```bash
# Iniciar
docker-compose up --build

# Iniciar en background
docker-compose up -d

# Ver logs
docker-compose logs -f

# Logs solo de la API
docker-compose logs -f api

# Detener
docker-compose down

# Detener y eliminar datos
docker-compose down -v

# Reconstruir imágenes
docker-compose build --no-cache

# Estado de servicios
docker-compose ps

# Acceder a contenedor API
docker exec -it ambientes-api sh

# Acceder a PostgreSQL
docker exec -it ambientes-postgres psql -U admin -d ambientes_db
```

## 🔧 .NET CLI

```bash
# Restaurar paquetes
dotnet restore

# Compilar
dotnet build

# Ejecutar
dotnet run

# Limpiar
dotnet clean

# Publicar
dotnet publish -c Release

# Crear migración EF
dotnet ef migrations add NombreMigracion --project src/Ambientes.Data

# Aplicar migraciones
dotnet ef database update --project src/Ambientes.Data

# Ver migraciones
dotnet ef migrations list --project src/Ambientes.Data

# Eliminar última migración
dotnet ef migrations remove --project src/Ambientes.Data

# Revertir a una migración anterior
dotnet ef database update NombreMigracion --project src/Ambientes.Data

# Generar script SQL
dotnet ef migrations script --project src/Ambientes.Data
```

## 🌐 API HTTP (cURL)

```bash
# Variables
API=http://localhost:8080

# Obtener todos
curl $API/api/ambientes

# Obtener por ID
curl $API/api/ambientes/1

# Crear
curl -X POST $API/api/ambientes \
  -H "Content-Type: application/json" \
  -d '{"codigo":"TEST-001","nombre":"Test","tipoAmbiente":"Aula","ubicacion":"Piso 1","estado":"Disponible"}'

# Actualizar
curl -X PUT $API/api/ambientes/1 \
  -H "Content-Type: application/json" \
  -d '{"codigo":"TEST-001","nombre":"Test Updated","tipoAmbiente":"Aula","ubicacion":"Piso 2","estado":"Ocupado"}'

# Eliminar
curl -X DELETE $API/api/ambientes/1

# Health check
curl $API/health

# Pretty JSON
curl $API/api/ambientes | jq .
```

## 📊 PostgreSQL

```bash
# Conectar a BD
psql -U admin -d ambientes_db

# Dentro de psql
-- Ver tablas
\dt

-- Ver estructura tabla
\d ambientes

-- Ver índices
\di

-- Contar registros
SELECT COUNT(*) FROM ambientes;

-- Ver todos
SELECT * FROM ambientes;

-- Buscar por código
SELECT * FROM ambientes WHERE codigo = 'LAB-001';

-- Ver por estado
SELECT * FROM ambientes WHERE estado = 'Disponible' ORDER BY nombre;

-- Eliminar todos los datos
DELETE FROM ambientes;

-- Resetear sequence ID
ALTER SEQUENCE ambientes_id_seq RESTART WITH 1;

-- Salir
\q
```

## 📁 Navegación de Carpetas

```bash
# Ir a la carpeta del proyecto
cd /ruta/a/Ambientes

# Ir a la API
cd src/Ambientes.API

# Ir a Servicios
cd ../Ambientes.Services

# Ir a Datos
cd ../Ambientes.Data

# Volver atrás
cd ../..

# Listar archivos
ls -la

# Ver estructura (tree)
tree src
```

## 🔍 Búsqueda y Grep

```bash
# Buscar archivo
find . -name "*.cs"

# Buscar texto en archivos
grep -r "public class" src/

# Contar líneas de código
find src -name "*.cs" -exec wc -l {} + | tail -1

# Buscar TODO comments
grep -r "TODO\|FIXME" src/
```

## 📝 Visual Studio Code

```bash
# Abrir VS Code
code .

# Abrir carpeta específica
code src/Ambientes.API

# Abrir archivo
code src/Ambientes.API/Program.cs
```

## 🔨 Compilación y Build

```bash
# Debug
dotnet build -c Debug

# Release
dotnet build -c Release

# Ver errores
dotnet build 2>&1 | grep error

# Info compilación
dotnet build --verbose
```

## 🧹 Limpieza

```bash
# Limpiar bin y obj
dotnet clean

# Limpiar completamente
rm -rf src/*/bin src/*/obj

# Limpiar Docker
docker system prune

# Limpiar imágenes sin usar
docker image prune

# Limpiar volúmenes sin usar
docker volume prune
```

## 📊 Monitoreo

```bash
# Ver recursos Docker
docker stats

# Ver logs con timestamps
docker-compose logs -t

# Log de últimas N líneas
docker-compose logs --tail=100

# Log de hace N segundos
docker-compose logs --since 2m
```

## 🔐 Seguridad

```bash
# Cambiar contraseña en appsettings
# Editar archivo manualmente

# Generar certificado SSL
dotnet dev-certs https --clean
dotnet dev-certs https --trust

# Ver certificados
dotnet dev-certs https --check
```

## 📦 NuGet

```bash
# Buscar paquete
dotnet package search EntityFrameworkCore

# Ver dependencias
dotnet list package

# Ver paquetes desactualizados
dotnet list package --outdated

# Actualizar paquete
dotnet add package NombrePaquete --version 8.0.0
```

## 🐛 Debugging

```bash
# Ejecutar con logging detallado
dotnet run --verbosity Debug

# Ver excepciones en detalle
dotnet run 2>&1

# Profiler (requiere IDE)
# Visual Studio: Debug → Performance Profiler
```

## 🚢 Despliegue

```bash
# Publicar para producción
dotnet publish -c Release -o ./publish

# Comprimir publish
tar -czf ambientes-release.tar.gz publish/

# Transferir a servidor
scp ambientes-release.tar.gz user@server:/opt/

# En servidor: descomprimir y ejecutar
tar -xzf ambientes-release.tar.gz
./publish/Ambientes.API
```

## 📋 Varios

```bash
# Ver versión de .NET
dotnet --version

# Ver SDK instalados
dotnet --list-sdks

# Ver runtimes
dotnet --list-runtimes

# Ver información del proyecto
dotnet project info

# Generar archivo de solución
dotnet new sln -n Ambientes

# Agregar proyecto a solución
dotnet sln add src/Ambientes.API/Ambientes.API.csproj

# Ver soluciones
dotnet sln list
```

---

**Tip**: Guarda este archivo en un lugar accesible para referencia rápida durante el desarrollo.
