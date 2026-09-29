using Microsoft.AspNetCore.Mvc;
using TapiaSolution.Shared.DTOs;
using TapiaSolution.Web.Services;

namespace TapiaSolution.Web.Controllers
{
    // ⚠️ SIN [Authorize] a nivel clase, porque el catálogo es público.
    // Pero las acciones internas verifican sesión manualmente.
    public class ClienteController : Controller
    {
        private readonly ApiClient _api;
        public ClienteController(ApiClient api) => _api = api;

        // 🔐 Helpers de sesión
        private bool EstaLogueado() =>
            !string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken"));

        private string? Rol() => HttpContext.Session.GetString("Rol");

        private string? Nombre() => HttpContext.Session.GetString("Nombre");

        private IActionResult RedirigirLogin(string returnUrl) =>
            RedirectToAction("Login", "Account", new { returnUrl });

        // ============================================================
        // 🏠 DASHBOARD DEL CLIENTE
        // ============================================================
        public async Task<IActionResult> Index()
        {
            if (!EstaLogueado())
                return RedirigirLogin(Url.Action("Index", "Cliente")!);

            if (Rol() != "Cliente")
                return RedirectToAction("Index", "Home");

            // Citas del cliente
            var citas = await _api.GetAsync<List<CitaDto>>("Citas/mis-citas") ?? new();

            // Resumen
            ViewBag.Nombre = Nombre();
            ViewBag.TotalCitas = citas.Count;
            ViewBag.CitasPendientes = citas.Count(c => c.Estado == "Pendiente");
            ViewBag.CitasConfirmadas = citas.Count(c => c.Estado == "Confirmada");

            // Últimas 3 citas para mostrar en el dashboard
            ViewBag.UltimasCitas = citas
                .OrderByDescending(c => c.FechaHora)
                .Take(3)
                .ToList();

            return View();
        }

        // ============================================================
        // 📅 MIS CITAS
        // ============================================================
        public async Task<IActionResult> MisCitas()
        {
            if (!EstaLogueado())
                return RedirigirLogin(Url.Action("MisCitas", "Cliente")!);

            if (Rol() != "Cliente")
                return RedirectToAction("Index", "Home");

            var citas = await _api.GetAsync<List<CitaDto>>("Citas/mis-citas") ?? new();

            // Ordenar por fecha descendente
            citas = citas.OrderByDescending(c => c.FechaHora).ToList();

            return View(citas);
        }

        // ============================================================
        // 📅 RESERVAR VISITA (GET)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Reservar(int propiedadId)
        {
            if (propiedadId <= 0)
                return RedirectToAction("Index", "Propiedades");

            if (!EstaLogueado())
                return RedirigirLogin(Url.Action("Reservar", "Cliente", new { propiedadId })!);

            if (Rol() != "Cliente")
                return RedirectToAction("Index", "Home");

            // Cargar la propiedad para mostrar sus datos en el formulario
            var propiedad = await _api.GetAsync<PropiedadDto>($"Propiedades/{propiedadId}");
            if (propiedad == null) return NotFound();

            ViewBag.Propiedad = propiedad;
            return View();
        }

        // ============================================================
        // 📅 RESERVAR VISITA (POST)
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reservar(CrearCitaDto dto)
        {
            if (!EstaLogueado())
                return RedirigirLogin(Url.Action("Reservar", "Cliente", new { propiedadId = dto.PropiedadId })!);

            if (Rol() != "Cliente")
                return RedirectToAction("Index", "Home");

            // Validar fecha futura
            if (dto.FechaHora <= DateTime.Now)
            {
                ModelState.AddModelError("", "La fecha y hora deben ser en el futuro.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Propiedad = await _api.GetAsync<PropiedadDto>($"Propiedades/{dto.PropiedadId}");
                return View(dto);
            }

            // Llamar a la API
            var resp = await _api.PostAsync("Citas", dto);

            if (!resp.IsSuccessStatusCode)
            {
                var error = await resp.Content.ReadAsStringAsync();
                ViewBag.Error = string.IsNullOrWhiteSpace(error)
                    ? "No se pudo reservar la cita. Intenta con otro horario."
                    : error;

                ViewBag.Propiedad = await _api.GetAsync<PropiedadDto>($"Propiedades/{dto.PropiedadId}");
                return View(dto);
            }

            TempData["Ok"] = "✅ ¡Cita reservada! El agente que publicó la propiedad se contactará contigo.";
            return RedirectToAction("MisCitas");
        }

        // ============================================================
        // ❌ CANCELAR CITA
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id)
        {
            if (!EstaLogueado())
                return RedirigirLogin(Url.Action("MisCitas", "Cliente")!);

            if (Rol() != "Cliente")
                return RedirectToAction("Index", "Home");

            // La API expone PUT /api/Citas/{id}/cancelar
            var resp = await _api.PutAsync($"Citas/{id}/cancelar", new { });

            TempData["Ok"] = resp.IsSuccessStatusCode
                ? "✅ Cita cancelada correctamente."
                : "❌ No se pudo cancelar la cita.";

            return RedirectToAction("MisCitas");
        }
    }
}