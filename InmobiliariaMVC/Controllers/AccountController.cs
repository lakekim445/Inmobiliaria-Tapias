using System.Security.Claims;
using InmobiliariaMVC.Models;
using InmobiliariaMVC.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace InmobiliariaMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApiService _apiService;

        public AccountController(ApiService apiService)
        {
            _apiService = apiService;
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

            // Llamar a la API
            var respuesta = await _apiService.PostAsync<LoginResponseDTO>(
                "auth/login",
                new { Email = model.Email, Password = model.Password });

            if (respuesta == null || string.IsNullOrEmpty(respuesta.Token))
            {
                ModelState.AddModelError("", "Email o contraseña incorrectos");
                return View(model);
            }

            // Guardar el JWT en sesión
            HttpContext.Session.SetString("JWT", respuesta.Token);
            HttpContext.Session.SetString("NombreCompleto", respuesta.NombreCompleto);
            HttpContext.Session.SetString("Rol", respuesta.Rol);

            // Crear los claims para la cookie
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, respuesta.NombreCompleto),
                new Claim(ClaimTypes.Email, respuesta.Email),
                new Claim(ClaimTypes.Role, respuesta.Rol)
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

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
            return respuesta.Rol switch
            {
                "Admin" => RedirectToAction("Index", "Admin"),
                "Agente" => RedirectToAction("Index", "Agente"),
                "Cliente" => RedirectToAction("Index", "Cliente"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        // ============================================================
        // GET: /Account/Register
        // ============================================================
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // ============================================================
        // POST: /Account/Register
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // Llamar a la API para registrar
            var resultado = await _apiService.PostAsync<LoginResponseDTO>(
                "auth/register",
                new
                {
                    NombreCompleto = model.NombreCompleto,
                    Email = model.Email,
                    Telefono = model.Telefono,
                    Password = model.Password
                });

            if (resultado == null)
            {
                ModelState.AddModelError("", "Error al registrar. Intenta de nuevo.");
                return View(model);
            }

            TempData["MensajeExito"] = "¡Registro exitoso! Ahora puedes iniciar sesión.";
            return RedirectToAction("Login");
        }

        // ============================================================
        // POST: /Account/Logout
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
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