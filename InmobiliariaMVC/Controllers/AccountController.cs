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


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Email == model.Email && u.Activo);

            if (usuario == null || usuario.PasswordHash != model.Password)
            {
                ModelState.AddModelError("", "Email o contraseña incorrectos");
                return View(model);
            }


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


            return usuario.Rol?.NombreRol switch
            {
                "Admin" => RedirectToAction("Index", "Admin"),
                "Agente" => RedirectToAction("Index", "Agente"),
                "Cliente" => RedirectToAction("Index", "Cliente"),
                _ => RedirectToAction("Index", "Home")
            };
        }


        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var emailExiste = await _context.Usuarios.AnyAsync(u => u.Email == model.Email);
            if (emailExiste)
            {
                ModelState.AddModelError("Email", "Este email ya está registrado");
                return View(model);
            }

     
            var nuevoUsuario = new Usuario
            {
                NombreCompleto = model.NombreCompleto,
                Email = model.Email,
                PasswordHash = model.Password,   
                Telefono = model.Telefono,
                FechaRegistro = DateTime.UtcNow,
                Activo = true,
                IdRol = 3                       
            };

            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

        
            TempData["MensajeExito"] = "¡Registro exitoso! Ahora puedes iniciar sesión.";
            return RedirectToAction("Login");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }


        public IActionResult AccesoDenegado()
        {
            return View();
        }
    }
}