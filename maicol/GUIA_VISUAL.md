# 🎯 GUÍA VISUAL - PASO A PASO

## Configuración Visual del Módulo Persona

Este documento muestra visualmente cómo configurar el módulo.

---

## PASO 1: Verificar .NET 8

### En PowerShell/Terminal:
```
> dotnet --version
8.0.x
```

✅ Si ves 8.0.x → Continúa
❌ Si no ves esto → Descarga .NET 8

---

## PASO 2: Verificar PostgreSQL

### Opción A: Desde PowerShell (Windows)
```powershell
> psql --version
psql (PostgreSQL) 12.x
```

### Opción B: Desde Terminal (Linux/Mac)
```bash
$ psql --version
psql (PostgreSQL) 12.x
```

✅ Si ves PostgreSQL → Continúa
❌ Si no ves esto → Descarga PostgreSQL

---

## PASO 3: Crear Base de Datos

### A. Abrir psql

**Windows:**
```powershell
> psql -h localhost -U postgres
Password: [INGRESA TU CONTRASEÑA]
```

**Linux/Mac:**
```bash
$ psql -h localhost -U postgres
Password: [INGRESA TU CONTRASEÑA]
```

### B. Crear Base de Datos

```sql
CREATE DATABASE sistema_personas 
  ENCODING 'UTF8' 
  LC_COLLATE 'es_ES.UTF-8' 
  LC_CTYPE 'es_ES.UTF-8';

\l  -- Ver bases de datos
```

Resultado esperado:
```
                                   List of databases
        Name         | Owner    | Encoding |   Collate     |    Ctype
--------------------+----------+----------+---------------+----------
 postgres           | postgres | UTF8     | es_ES.UTF-8   | es_ES.UTF-8
 sistema_personas   | postgres | UTF8     | es_ES.UTF-8   | es_ES.UTF-8
```

### C. Salir de psql
```sql
\q
```

---

## PASO 4: Crear Tabla

### A. Desde PowerShell/Terminal

```bash
psql -h localhost -U postgres -d sistema_personas -f sql_script_crear_tabla.sql
```

O manualmente:

```bash
psql -h localhost -U postgres -d sistema_personas
```

### B. Crear tabla

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

CREATE INDEX idx_personas_email ON personas(email);
CREATE INDEX idx_personas_ci ON personas(ci);
CREATE INDEX idx_personas_codigo ON personas(codigo);

\d personas  -- Ver estructura
```

Resultado esperado:
```
                        Table "public.personas"
      Column       |       Type        |      Modifiers
------------------+-------------------+--------------------
 id_persona       | integer           | not null DEFAULT...
 nombres          | character varying | not null
 apellido_paterno | character varying | not null
 apellido_materno | character varying | not null
 email            | character varying | not null
 telefono         | character varying | not null
 fecha_nac        | date              | not null
 tipo             | character varying | not null
 codigo           | character varying | not null
 ci               | character varying | not null
```

---

## PASO 5: Copiar Archivos

### Estructura esperada después de copiar:

```
SistemaWeb/
├── Models/
│   └── Persona.cs ✅
├── Data/
│   ├── ApplicationDbContext.cs ✅
│   └── DbInitializer.cs ✅
├── Services/
│   └── PersonaService.cs ✅
├── Controllers/
│   └── PersonaController.cs ✅
├── Views/
│   ├── Persona/
│   │   ├── Index.cshtml ✅
│   │   ├── Create.cshtml ✅
│   │   ├── Edit.cshtml ✅
│   │   ├── Details.cshtml ✅
│   │   └── Delete.cshtml ✅
│   └── Shared/
│       └── _Layout.cshtml (personalizar)
├── Program.cs ✅
├── appsettings.json ✅
├── appsettings.Development.json ✅
└── SistemaWeb.csproj ✅
```

---

## PASO 6: Editar appsettings.json

### En Visual Studio Code:

**Archivo:** `appsettings.json`

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

**IMPORTANTE:** Reemplace `Password=postgres` con su contraseña real

---

## PASO 7: Instalar Paquetes

### Opción A: Línea de Comandos

```bash
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.EntityFrameworkCore.Tools
```

### Opción B: Visual Studio NuGet

1. Clic derecho en Proyecto
2. Manage NuGet Packages
3. Buscar: `Npgsql.EntityFrameworkCore.PostgreSQL`
4. Instalar versión 8.0.x
5. Repetir para `Microsoft.EntityFrameworkCore.Tools`

---

## PASO 8: Compilar Proyecto

```bash
dotnet build
```

Resultado esperado:
```
Build started...
...
✓ Build succeeded. 0 Warning(s)
```

Si hay errores:
1. Revise que appsettings.json sea JSON válido
2. Revise que todos los archivos estén copiados
3. Revise que los paquetes estén instalados

---

## PASO 9: Ejecutar Aplicación

```bash
dotnet run
```

Resultado esperado:
```
🚀 Iniciando aplicación ASP.NET Core MVC...
📁 Entorno: Development
info: Microsoft.Hosting.Lifetime[0]
      Now listening on: https://localhost:7000
```

---

## PASO 10: Probar en Navegador

### Abra el navegador en:
```
https://localhost:7000/Persona
```

Debería ver:

```
┌─────────────────────────────────────────┐
│  👥 Gestión de Personas                 │
│                                         │
│  📝 [Buscar...]           [+ Nueva]     │
│                                         │
│  │ ID │ Nombre       │ Email         │  │
│  ├────┼──────────────┼──────────────┤  │
│  │    │ No hay datos │              │  │
│  └────┴──────────────┴──────────────┘  │
│                                         │
│  [Total: 0]                             │
└─────────────────────────────────────────┘
```

✅ Si ves esto → Éxito
❌ Si ves error → Revise los logs

---

## PASO 11: Crear Primera Persona

### 1. Haga clic en "Nueva Persona"

```
┌─────────────────────────────────────────┐
│  ➕ Crear Nueva Persona                  │
│                                         │
│  📋 Información Personal                │
│                                         │
│  Nombres: [Juan Carlos          ]      │
│  Apel. Paterno: [García         ]      │
│  Apel. Materno: [López          ]      │
│  Fecha Nac: [1990-05-15         ]      │
│                                         │
│  📧 Información de Contacto             │
│                                         │
│  Email: [juan@ejemplo.com       ]      │
│  Teléfono: [+591 76543210      ]      │
│                                         │
│  🆔 Identificación                      │
│                                         │
│  Tipo: [Documento ▼]                   │
│  Código: [PERS001            ]         │
│  CI: [1234567               ]          │
│                                         │
│  [💾 Guardar Persona]  [❌ Cancelar]    │
└─────────────────────────────────────────┘
```

### 2. Complete los campos:

```
Nombres:           Juan Carlos
Apel. Paterno:     García
Apel. Materno:     López
Email:             juan.garcia@ejemplo.com
Teléfono:          +591 76543210
Fecha Nac:         1990-05-15
Tipo:              Documento
Código:            PERS001
CI:                1234567
```

### 3. Haga clic en "Guardar Persona"

### 4. Debería ver:

```
✅ Persona 'Juan Carlos García López' creada exitosamente.

│ ID │ Nombre                    │ Email                │...│
├────┼──────────────────────────┼────────────────────┤   │
│ 1  │ Juan Carlos García López │ juan.garcia@ex...  │...│
```

---

## PASO 12: Verificar en Base de Datos

```bash
psql -h localhost -U postgres -d sistema_personas

SELECT * FROM personas;
```

Resultado:
```
 id_persona │   nombres    │ apellido_paterno │ apellido_materno │
            │              │                  │                  │
-----------┼──────────────┼──────────────────┼──────────────────┤
           1 │ Juan Carlos  │ García          │ López            │
```

✅ Persona guardada correctamente

---

## ✨ ¡ÉXITO! ¡Ya funciona!

Si llegaste hasta aquí, tu módulo Persona está completamente funcional.

Ahora puedes:
- ✅ Crear personas
- ✅ Listar personas
- ✅ Buscar personas
- ✅ Editar personas
- ✅ Eliminar personas
- ✅ Filtrar por tipo

---

## 🆘 Troubleshooting Rápido

### ❌ Error: Connection refused
```
Npgsql.NpgsqlException: Unable to connect
```
**Solución:**
```bash
# Verificar PostgreSQL
psql -h localhost -U postgres

# Si no funciona, inicie PostgreSQL
# Windows: Servicios → PostgreSQL → Reiniciar
```

### ❌ Error: Database does not exist
```
FATAL: database "sistema_personas" does not exist
```
**Solución:**
```bash
psql -h localhost -U postgres
CREATE DATABASE sistema_personas;
\c sistema_personas
[Crear tabla aquí]
```

### ❌ Error: Invalid password
```
FATAL: password authentication failed
```
**Solución:** Revise la contraseña en `appsettings.json`

### ❌ No ve datos
**Solución:**
```bash
# Verificar que tabla existe
psql -h localhost -U postgres -d sistema_personas
\d personas
```

---

## 📞 ¿Necesitas más ayuda?

1. Revisa **README.md**
2. Revisa **GUIA_INSTALACION.md**
3. Revisa **FAQ.md**
4. Revisa los logs en consola

---

**¡Éxito con tu módulo! 🎉**
