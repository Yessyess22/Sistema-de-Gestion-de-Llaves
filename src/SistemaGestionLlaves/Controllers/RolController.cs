using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestionLlaves.Data;
using SistemaGestionLlaves.Models;

namespace SistemaGestionLlaves.Controllers;

/// <summary>
/// Controlador para la gestión de Roles del sistema.
/// Proporciona acciones CRUD para crear, leer, actualizar y eliminar roles.
/// </summary>
public class RolController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<RolController> _logger;

    public RolController(ApplicationDbContext context, ILogger<RolController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ============================================================
    // INDEX - Listar todos los roles
    // ============================================================
    /// <summary>
    /// Obtiene la lista de todos los roles (activos e inactivos).
    /// </summary>
    /// <returns>Vista con lista de roles</returns>
    public async Task<IActionResult> Index()
    {
        try
        {
            var roles = await _context.Roles
                .Include(r => r.RolPermisos)
                .ThenInclude(rp => rp.Permiso)
                .Include(r => r.Usuarios)
                .OrderBy(r => r.NombreRol)
                .ToListAsync();

            _logger.LogInformation("Se consultaron los roles del sistema");
            return View(roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener la lista de roles");
            TempData["Error"] = "Error al cargar los roles. Por favor, intente más tarde.";
            return RedirectToAction("Error", "Home");
        }
    }

    // ============================================================
    // CREATE GET - Formulario para crear un nuevo rol
    // ============================================================
    /// <summary>
    /// Muestra el formulario para crear un nuevo rol.
    /// </summary>
    /// <returns>Vista del formulario de creación</returns>
    public async Task<IActionResult> Create()
    {
        try
        {
            // Obtener permisos disponibles para asignarlos al rol
            var permisos = await _context.Permisos
                .OrderBy(p => p.NombrePermiso)
                .ToListAsync();

            ViewData["Permisos"] = permisos;
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al preparar el formulario de creación de rol");
            TempData["Error"] = "Error al preparar el formulario.";
            return RedirectToAction(nameof(Index));
        }
    }

    // ============================================================
    // CREATE POST - Guardar un nuevo rol
    // ============================================================
    /// <summary>
    /// Crea un nuevo rol con los datos proporcionados.
    /// </summary>
    /// <param name="rol">Modelo del rol a crear</param>
    /// <param name="permisosSeleccionados">IDs de permisos a asignar</param>
    /// <returns>Redirecciona al índice o vuelve a la vista en caso de error</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("NombreRol,Descripcion,Estado")] Rol rol, [FromForm] int[] permisosSeleccionados)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState inválido al intentar crear un rol");
                var permisos = await _context.Permisos.ToListAsync();
                ViewData["Permisos"] = permisos;
                return View(rol);
            }

            // Validar que el nombre del rol sea único
            var rolExistente = await _context.Roles
                .FirstOrDefaultAsync(r => r.NombreRol.ToLower() == rol.NombreRol.ToLower());

            if (rolExistente != null)
            {
                ModelState.AddModelError("NombreRol", "Ya existe un rol con este nombre.");
                var permisos = await _context.Permisos.ToListAsync();
                ViewData["Permisos"] = permisos;
                return View(rol);
            }

            // Establecer estado por defecto si no viene especificado
            if (string.IsNullOrEmpty(rol.Estado))
            {
                rol.Estado = "A";
            }

            // Validar estado
            if (rol.Estado != "A" && rol.Estado != "I")
            {
                ModelState.AddModelError("Estado", "El estado debe ser 'A' (Activo) o 'I' (Inactivo).");
                var permisos = await _context.Permisos.ToListAsync();
                ViewData["Permisos"] = permisos;
                return View(rol);
            }

            // Agregar el rol
            _context.Add(rol);
            await _context.SaveChangesAsync();

            // Asignar permisos al rol si se seleccionaron
            if (permisosSeleccionados != null && permisosSeleccionados.Length > 0)
            {
                foreach (var idPermiso in permisosSeleccionados)
                {
                    var permisoExiste = await _context.Permisos.AnyAsync(p => p.IdPermiso == idPermiso);
                    if (permisoExiste)
                    {
                        var rolPermiso = new RolPermiso
                        {
                            IdRol = rol.IdRol,
                            IdPermiso = idPermiso
                        };
                        _context.Add(rolPermiso);
                    }
                }
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation($"Rol '{rol.NombreRol}' creado exitosamente. ID: {rol.IdRol}");
            TempData["Exito"] = $"El rol '{rol.NombreRol}' ha sido creado exitosamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error de base de datos al crear el rol");
            ModelState.AddModelError("", "Error al guardar en la base de datos. Por favor, intente de nuevo.");
            var permisos = await _context.Permisos.ToListAsync();
            ViewData["Permisos"] = permisos;
            return View(rol);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al crear el rol");
            TempData["Error"] = "Error inesperado al crear el rol. Por favor, intente más tarde.";
            return RedirectToAction(nameof(Index));
        }
    }

    // ============================================================
    // EDIT GET - Formulario para editar un rol
    // ============================================================
    /// <summary>
    /// Muestra el formulario para editar un rol existente.
    /// </summary>
    /// <param name="id">ID del rol a editar</param>
    /// <returns>Vista del formulario de edición</returns>
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        try
        {
            var rol = await _context.Roles
                .Include(r => r.RolPermisos)
                .FirstOrDefaultAsync(r => r.IdRol == id);

            if (rol == null)
            {
                _logger.LogWarning($"Se intentó editar un rol inexistente. ID: {id}");
                return NotFound();
            }

            var permisos = await _context.Permisos
                .OrderBy(p => p.NombrePermiso)
                .ToListAsync();

            var permisosAsignados = await _context.RolPermisos
                .Where(rp => rp.IdRol == id)
                .Select(rp => rp.IdPermiso)
                .ToListAsync();

            ViewData["Permisos"] = permisos;
            ViewData["PermisosAsignados"] = permisosAsignados;

            return View(rol);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al cargar el formulario de edición del rol {id}");
            TempData["Error"] = "Error al cargar el formulario de edición.";
            return RedirectToAction(nameof(Index));
        }
    }

    // ============================================================
    // EDIT POST - Guardar cambios en un rol
    // ============================================================
    /// <summary>
    /// Actualiza un rol existente con los datos proporcionados.
    /// </summary>
    /// <param name="id">ID del rol a actualizar</param>
    /// <param name="rol">Modelo del rol con los datos actualizados</param>
    /// <param name="permisosSeleccionados">IDs de permisos a asignar</param>
    /// <returns>Redirecciona al índice o vuelve a la vista en caso de error</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("IdRol,NombreRol,Descripcion,Estado")] Rol rol, [FromForm] int[] permisosSeleccionados)
    {
        if (id != rol.IdRol)
        {
            return NotFound();
        }

        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning($"ModelState inválido al intentar actualizar el rol {id}");
                var permisos = await _context.Permisos.ToListAsync();
                ViewData["Permisos"] = permisos;
                return View(rol);
            }

            // Validar unicidad del nombre si ha cambiado
            var rolExistente = await _context.Roles
                .FirstOrDefaultAsync(r => r.NombreRol.ToLower() == rol.NombreRol.ToLower() && r.IdRol != id);

            if (rolExistente != null)
            {
                ModelState.AddModelError("NombreRol", "Ya existe otro rol con este nombre.");
                var permisos = await _context.Permisos.ToListAsync();
                ViewData["Permisos"] = permisos;
                return View(rol);
            }

            // Validar estado
            if (rol.Estado != "A" && rol.Estado != "I")
            {
                ModelState.AddModelError("Estado", "El estado debe ser 'A' (Activo) o 'I' (Inactivo).");
                var permisos = await _context.Permisos.ToListAsync();
                ViewData["Permisos"] = permisos;
                return View(rol);
            }

            _context.Update(rol);
            await _context.SaveChangesAsync();

            // Actualizar permisos: eliminar los actuales y agregar los nuevos
            var permisosActuales = await _context.RolPermisos
                .Where(rp => rp.IdRol == id)
                .ToListAsync();

            foreach (var rp in permisosActuales)
            {
                _context.RolPermisos.Remove(rp);
            }
            await _context.SaveChangesAsync();

            if (permisosSeleccionados != null && permisosSeleccionados.Length > 0)
            {
                foreach (var idPermiso in permisosSeleccionados)
                {
                    var permisoExiste = await _context.Permisos.AnyAsync(p => p.IdPermiso == idPermiso);
                    if (permisoExiste)
                    {
                        var rolPermiso = new RolPermiso
                        {
                            IdRol = rol.IdRol,
                            IdPermiso = idPermiso
                        };
                        _context.Add(rolPermiso);
                    }
                }
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation($"Rol '{rol.NombreRol}' (ID: {id}) actualizado exitosamente");
            TempData["Exito"] = $"El rol '{rol.NombreRol}' ha sido actualizado exitosamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, $"Error de concurrencia al actualizar el rol {id}");
            TempData["Error"] = "El rol ha sido modificado por otro usuario. Por favor, intente de nuevo.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error inesperado al actualizar el rol {id}");
            TempData["Error"] = "Error inesperado al actualizar el rol. Por favor, intente más tarde.";
            return RedirectToAction(nameof(Index));
        }
    }

    // ============================================================
    // DELETE GET - Confirmación de eliminación
    // ============================================================
    /// <summary>
    /// Muestra la información del rol a eliminar para confirmación.
    /// </summary>
    /// <param name="id">ID del rol a eliminar</param>
    /// <returns>Vista de confirmación de eliminación</returns>
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        try
        {
            var rol = await _context.Roles
                .Include(r => r.RolPermisos)
                .Include(r => r.Usuarios)
                .FirstOrDefaultAsync(r => r.IdRol == id);

            if (rol == null)
            {
                _logger.LogWarning($"Se intentó eliminar un rol inexistente. ID: {id}");
                return NotFound();
            }

            return View(rol);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al cargar la confirMación de eliminación del rol {id}");
            TempData["Error"] = "Error al cargar la confirmación de eliminación.";
            return RedirectToAction(nameof(Index));
        }
    }

    // ============================================================
    // DELETE POST - Eliminar un rol
    // ============================================================
    /// <summary>
    /// Elimina un rol del sistema.
    /// Verifica que el rol no esté asociado a usuarios antes de eliminarlo.
    /// </summary>
    /// <param name="id">ID del rol a eliminar</param>
    /// <returns>Redirecciona al índice</returns>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var rol = await _context.Roles
                .Include(r => r.Usuarios)
                .FirstOrDefaultAsync(r => r.IdRol == id);

            if (rol == null)
            {
                _logger.LogWarning($"Se intentó eliminar un rol inexistente. ID: {id}");
                TempData["Error"] = "El rol no existe.";
                return RedirectToAction(nameof(Index));
            }

            // Validar que el rol no tenga usuarios asignados
            if (rol.Usuarios != null && rol.Usuarios.Count > 0)
            {
                _logger.LogWarning($"Se intentó eliminar un rol con usuarios asignados. Rol ID: {id}");
                TempData["Error"] = $"No se puede eliminar el rol '{rol.NombreRol}' porque tiene {rol.Usuarios.Count} usuario(s) asignado(s).";
                return RedirectToAction(nameof(Index));
            }

            _context.Roles.Remove(rol);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Rol '{rol.NombreRol}' (ID: {id}) eliminado exitosamente");
            TempData["Exito"] = $"El rol '{rol.NombreRol}' ha sido eliminado exitosamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, $"Error de base de datos al eliminar el rol {id}");
            TempData["Error"] = "Error al eliminar el rol. Puede estar siendo utilizado por otros registros.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error inesperado al eliminar el rol {id}");
            TempData["Error"] = "Error inesperado al eliminar el rol. Por favor, intente más tarde.";
            return RedirectToAction(nameof(Index));
        }
    }

    // ============================================================
    // DETAILS - Detalles de un rol
    // ============================================================
    /// <summary>
    /// Muestra los detalles y permisos de un rol específico.
    /// </summary>
    /// <param name="id">ID del rol</param>
    /// <returns>Vista con los detalles del rol</returns>
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        try
        {
            var rol = await _context.Roles
                .Include(r => r.RolPermisos)
                .ThenInclude(rp => rp.Permiso)
                .Include(r => r.Usuarios)
                .FirstOrDefaultAsync(r => r.IdRol == id);

            if (rol == null)
            {
                _logger.LogWarning($"Se intentó ver detalles de un rol inexistente. ID: {id}");
                return NotFound();
            }

            return View(rol);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al obtener detalles del rol {id}");
            TempData["Error"] = "Error al cargar los detalles del rol.";
            return RedirectToAction(nameof(Index));
        }
    }
}
