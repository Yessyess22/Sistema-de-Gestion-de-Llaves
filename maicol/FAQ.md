# ❓ PREGUNTAS FRECUENTES (FAQ)

## Módulo PERSONA - ASP.NET Core MVC

---

## 🔴 PREGUNTAS SOBRE INSTALACIÓN

### P1: ¿Cómo verifico si tengo .NET 8 instalado?

```bash
dotnet --version
```

Debe mostrar `8.0.x`. Si no, [descargue .NET 8](https://dotnet.microsoft.com/download/dotnet/8.0)

---

### P2: ¿Necesito crear la tabla en PostgreSQL manualmente?

**Respuesta:** Sí, la tabla debe existir en PostgreSQL. Ejecute:

```bash
psql -h localhost -U postgres -d su_base_datos -f sql_script_crear_tabla.sql
```

O manualmente en pgAdmin:

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

### P3: ¿Puedo usar Entity Framework Migrations?

**Respuesta:** No, el módulo está diseñado para mapear una tabla existente. Si desea usar migraciones:

1. Elimine la tabla existente
2. Cree una migración:
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

---

### P4: ¿Qué versión de PostgreSQL necesito?

**Respuesta:** Mínimo PostgreSQL 12. Versiones recomendadas: 12, 13, 14, 15.

Verifique:
```bash
psql --version
```

---

## 🟢 PREGUNTAS SOBRE CONFIGURACIÓN

### P5: ¿Dónde pongo la cadena de conexión?

**Respuesta:** En `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=mi_base;Username=postgres;Password=contraseña;Encoding=UTF8"
  }
}
```

---

### P6: ¿Qué pasa si uso contraseña con caracteres especiales?

**Respuesta:** Debe codificar la URL. Por ejemplo, si la contraseña es `p@ssw0rd#123`:

```
Password=p%40ssw0rd%23123
```

O mejor aún, use variables de entorno en producción.

---

### P7: ¿Puedo cambiar el nombre de la tabla?

**Respuesta:** Sí. En `ApplicationDbContext.cs`, cambie:

```csharp
[Table("personas")]  // Cambie aquí
public class Persona
```

Y también en la anotación:

```csharp
entity.ToTable("mi_tabla_personalizada");
```

---

### P8: ¿Necesito cambiar los nombres de las columnas?

**Respuesta:** No, los nombres de columnas están mapeados con anotaciones:

```csharp
[Column("id_persona")]
public int IdPersona { get; set; }
```

Si su tabla tiene diferentes nombres, ajuste aquí.

---

## 🟡 PREGUNTAS SOBRE VALIDACIONES

### P9: ¿Dónde están las validaciones?

**Respuesta:** En dos lugares:

1. **Modelo (`Models/Persona.cs`)** - Data Annotations
2. **Servicio (`Services/PersonaService.cs`)** - Validaciones de negocio
3. **Vistas** - Validación en cliente (jQuery)

---

### P10: ¿Puedo cambiar las validaciones?

**Respuesta:** Sí. Edite las anotaciones en `Models/Persona.cs`:

```csharp
[Required(ErrorMessage = "Mensaje personalizado")]
[StringLength(150, MinimumLength = 2, ErrorMessage = "Custom...")]
public string Nombres { get; set; }
```

---

### P11: ¿Qué significan los errores de validación?

| Error | Significado | Solución |
|-------|-----------|----------|
| "Los nombres son requeridos" | Campo vacío | Ingrese algo |
| "Debe proporcionar un correo electrónico válido" | Email inválido | Use formato: user@domain.com |
| "La persona debe ser mayor de 18 años" | Menor de edad | Seleccione fecha correcta |
| "Ya existe una persona con el correo" | Email duplicado | Use otro email |
| "La Cédula de Identidad ya existe" | CI duplicado | Use otro CI |

---

## 🔵 PREGUNTAS SOBRE USO

### P12: ¿Cómo busco una persona?

**Respuesta:** En el listado (`/Persona`):

1. Escriba el nombre en el campo de búsqueda
2. Presione "Buscar"
3. Se filtrarán por nombres o apellidos

---

### P13: ¿Cómo filtro por tipo (Documento/Empresa)?

**Respuesta:** En el listado, use los botones de filtro:
- "Todos" - Todas las personas
- "Documentos" - Solo personas naturales
- "Empresas" - Solo empresas

---

### P14: ¿Qué es la "edad" que aparece?

**Respuesta:** Es una propiedad calculada de la fecha de nacimiento:

```csharp
public int Edad
{
    get { return DateTime.Today.Year - FechaNac.Year; }
}
```

Se calcula automáticamente, no se guarda.

---

### P15: ¿Qué es el "nombre completo"?

**Respuesta:** Es la concatenación de nombres y apellidos:

```csharp
public string NombreCompleto => 
    $"{Nombres} {ApellidoPaterno} {ApellidoMaterno}";
```

Ej: "Juan Carlos García López"

---

## 🟣 PREGUNTAS SOBRE PERSONALIZACIÓN

### P16: ¿Cómo agrego más campos?

**Respuesta:** 

1. Agregue la columna en PostgreSQL:
   ```sql
   ALTER TABLE personas ADD COLUMN nuevo_campo VARCHAR(100);
   ```

2. Agregue propiedad en `Models/Persona.cs`:
   ```csharp
   [Column("nuevo_campo")]
   [StringLength(100)]
   public string NuevoCampo { get; set; }
   ```

3. Agregue el campo en las vistas

---

### P17: ¿Cómo cambio los estilos de Bootstrap?

**Respuesta:** Personalice en `_Layout.cshtml`:

```html
<style>
    body { background-color: #e3f2fd; }  /* Azul claro */
    .navbar { background-color: #1976d2; }  /* Azul oscuro */
    .btn-primary { background-color: #ff6f00; }  /* Naranja */
</style>
```

O use Bootstrap utilities directamente en las vistas.

---

### P18: ¿Cómo elimino campos del formulario?

**Respuesta:** 

1. Comente o elimine en `Create.cshtml`/`Edit.cshtml`:
   ```html
   @* <div class="mb-3">
        <label asp-for="Campo">Campo</label>
        <input asp-for="Campo" class="form-control" />
   </div> *@
   ```

2. También en `PersonaController.cs`, remove del `[Bind]`:
   ```csharp
   [Bind("IdPersona,Nombres,...")]
   ```

---

## 🟠 PREGUNTAS SOBRE ERRORES

### P19: ¿Qué hacer si veo "Object reference not set"?

**Respuesta:** 

```csharp
// MALO - puede causar null reference
var persona = await _personaService.ObtenerPorIdAsync(id);
var nombre = persona.Nombres;  // Error si persona es null

// BUENO - verificar null
if (persona == null) return NotFound();
var nombre = persona.Nombres;
```

---

### P20: ¿Qué significa "DbUpdateException"?

**Respuesta:** Error al guardar en base de datos. Causas comunes:

1. Email duplicado
2. CI duplicado
3. Campo requerido vacío
4. Tipo de dato incorrecto
5. Restricción de clave foránea

Revise los logs para más detalles.

---

### P21: ¿Por qué me sale "404 Not Found"?

**Respuesta:** Causas:

1. ID de persona no existe
2. Ruta incorrecta (debe ser `/Persona/...` con capital P)
3. Controlador no registrado en `Program.cs`
4. Vista no existe

Verifique:
```
/Persona/Index        ✅ Correcto
/persona/index        ❌ Incorrecto (minúsculas)
/Personas/Index       ❌ Incorrecto (plural)
```

---

## 🟤 PREGUNTAS SOBRE RENDIMIENTO

### P22: ¿Cómo mejoro el rendimiento con muchos registros?

**Respuesta:**

1. Agregue índices (ya están en el modelo)
2. Implemente paginación:
   ```csharp
   var personas = await _context.Personas
       .Skip((page - 1) * pageSize)
       .Take(pageSize)
       .ToListAsync();
   ```

3. Use `AsNoTracking()` para consultas de solo lectura:
   ```csharp
   _context.Personas.AsNoTracking().ToListAsync()
   ```

---

### P23: ¿Qué son los índices en la base de datos?

**Respuesta:** Mejoran velocidad de búsqueda. Ya están creados:

```sql
CREATE INDEX idx_personas_email ON personas(email);
CREATE INDEX idx_personas_ci ON personas(ci);
```

---

## ⚫ PREGUNTAS SOBRE SEGURIDAD

### P24: ¿Cómo protejo los datos sensibles?

**Respuesta:**

1. **Contraseña de BD:** Use variables de entorno en producción
   ```bash
   $env:ConnectionString = "Host=..."
   ```

2. **Validación:** Las validaciones previenen inyección SQL

3. **HTTPS:** La app usa HTTPS en desarrollo

4. **CSRF:** Token automático en formularios

---

### P25: ¿Qué es el token AntiForgery?

**Respuesta:** Protege contra ataques CSRF. Se genera automáticamente:

```html
@Html.AntiForgeryToken()
```

**No lo elimine** de los formularios POST/PUT/DELETE.

---

## ⚪ PREGUNTAS SOBRE DEPLOYMENT

### P26: ¿Cómo publico la aplicación?

**Respuesta:** En Visual Studio:

1. Clic derecho en proyecto → Publish
2. Seleccione destino (Azure, IIS, carpeta local)
3. Configure la cadena de conexión de producción
4. Siga los pasos

O desde línea de comandos:

```bash
dotnet publish -c Release -o ./publish
```

---

### P27: ¿Cómo cambio la cadena de conexión en producción?

**Respuesta:** Cree `appsettings.Production.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=prod-server;Port=5432;Database=personas_prod;Username=prod_user;Password=prod_pass;Encoding=UTF8"
  }
}
```

O use variables de entorno:

```bash
set ASPNETCORE_ENVIRONMENT=Production
set ConnectionStrings__DefaultConnection=Host=...
```

---

## 📞 ¿No encuentras tu pregunta?

Si tu pregunta no está aquí:

1. Revisa el README.md
2. Revisa la GUIA_INSTALACION.md
3. Revisa los comentarios en el código
4. Revisa los logs de la aplicación

---

## 💡 CONSEJOS PROFESIONALES

### Consejo 1: Siempre limpie antes de compilar
```bash
dotnet clean
dotnet build
```

### Consejo 2: Revise los logs
Los errores normalmente están en la consola. ¡Léalos!

### Consejo 3: Use migraciones en equipo
```bash
dotnet ef migrations add MigracionNombre
dotnet ef database update
```

### Consejo 4: Teste manualmente primero
Antes de automatizar, prueba cada funcionalidad manualmente.

### Consejo 5: Documenta tus cambios
Si personalizas el código, deja comentarios para los demás.

---

**¡Gracias por usar el módulo Persona! 🎉**

Si tienes sugerencias o encuentras bugs, repórtalos.
