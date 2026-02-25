using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestionLlaves.Data;
using SistemaGestionLlaves.Models;
using System.Security.Claims;
using BCrypt.Net;

namespace SistemaGestionLlaves.Controllers;

/// <summary>
/// Controlador responsable de la autenticación de usuarios (Login, Logout y Acceso Denegado).
/// </summary>
public class CuentaController : Controller
{
    private readonly ApplicationDbContext _context;

    public CuentaController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Muestra la vista de inicio de sesión. Si el usuario ya está autenticado, redirige al índice de Usuarios.
    /// </summary>
    /// <returns>Vista de Login o Redirección</returns>
    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Index", "Usuarios");
        }
        return View();
    }

    /// <summary>
    /// Procesa el formulario de inicio de sesión, validando credenciales y creando la cookie de sesión.
    /// </summary>
    /// <param name="username">Nombre de usuario</param>
    /// <param name="password">Contraseña en texto plano</param>
    /// <returns>Vista de Login o redirección al Index de Usuarios en caso de éxito</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string username, string password)
    {
        // Validación básica de campos vacíos
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ViewBag.Error = "Debe ingresar usuario y contraseña.";
            return View();
        }

        // Buscar el usuario en la base de datos incluyendo sus relaciones principales
        var usuario = await _context.Usuarios
            .Include(u => u.Rol)
            .Include(u => u.Persona)
            .FirstOrDefaultAsync(u => u.NombreUsuario == username);

        if (usuario == null)
        {
            ViewBag.Error = "Usuario no encontrado.";
            return View();
        }

        // Verificar el estado del usuario (sólo usuarios 'Activos' pueden ingresar)
        if (usuario.Estado != "A")
        {
            ViewBag.Error = "El usuario no está activo.";
            return View();
        }

        // Verificar el hash de la contraseña usando BCrypt
        bool match = BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash);

        if (!match)
        {
            ViewBag.Error = "Contraseña incorrecta.";
            return View();
        }

        // Crear los Claims (afirmaciones) que se guardarán en la cookie de sesión
        var claims = new List<System.Security.Claims.Claim>
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, usuario.NombreUsuario),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, usuario.Rol.Nombre),
            new System.Security.Claims.Claim("IdPersona", usuario.IdPersona.ToString())
        };

        var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");

        // Registrar el inicio de sesión del usuario
        await HttpContext.SignInAsync(
            "CookieAuth",
            new ClaimsPrincipal(claimsIdentity));

        // Redirigir a la lista de usuarios tras iniciar sesión exitosamente
        return RedirectToAction("Index", "Usuarios");
    }

    /// <summary>
    /// Cierra la sesión activa del usuario actual.
    /// </summary>
    /// <returns>Redirección al Login</returns>
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("CookieAuth");
        return RedirectToAction("Login", "Cuenta");
    }

    /// <summary>
    /// Muestra una vista informando al usuario que no tiene permisos para acceder a un recurso.
    /// </summary>
    /// <returns>Vista de acceso denegado</returns>
    public IActionResult Denegado()
    {
        return View();
    }
}
