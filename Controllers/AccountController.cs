using System.Security.Claims;
using InmobiliariaMVC.Data;
using InmobiliariaMVC.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InmobiliariaMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly InmobiliariaContext _context;

        public AccountController(InmobiliariaContext context)
        {
            _context = context;
        }

        // ============================================================
        // GET: /Account/Login
        // ============================================================
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // ============================================================
        // POST: /Account/Login
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Email == model.Email && u.Activo);

            // Comparación directa en texto plano (SIN BCrypt)
            if (usuario == null || usuario.PasswordHash != model.Password)
            {
                ModelState.AddModelError("", "Email o contraseña incorrectos");
                return View(model);
            }

            // Crear los claims del usuario (van dentro de la cookie)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.NombreCompleto ?? ""),
                new Claim(ClaimTypes.Email, usuario.Email ?? ""),
                new Claim(ClaimTypes.Role, usuario.Rol?.NombreRol ?? "")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.Recordarme,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Redirigir según el rol
            return usuario.Rol?.NombreRol switch
            {
                "Admin" => RedirectToAction("Index", "Admin"),
                "Agente" => RedirectToAction("Index", "Agente"),
                "Cliente" => RedirectToAction("Index", "Cliente"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        // ============================================================
        // POST: /Account/Logout
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        // ============================================================
        // GET: /Account/AccesoDenegado
        // ============================================================
        public IActionResult AccesoDenegado()
        {
            return View();
        }
    }
}