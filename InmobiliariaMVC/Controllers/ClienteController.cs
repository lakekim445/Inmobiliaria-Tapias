using InmobiliariaMVC.Models;
using InmobiliariaMVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InmobiliariaMVC.Controllers
{
    // ✅ Solo clientes logueados pueden entrar al panel y reservar
    [Authorize(Roles = "Cliente")]
    public class ClienteController : Controller
    {
        private readonly ApiService _apiService;

        public ClienteController(ApiService apiService)
        {
            _apiService = apiService;
        }

        // ============================================================
        // 🏠 DASHBOARD DEL CLIENTE
        // ============================================================
        public async Task<IActionResult> Index()
        {
            var citas = await _apiService.GetAsync<List<CitaResumenDTO>>(
                "api/ClienteApi/citas") ?? new List<CitaResumenDTO>();

            ViewBag.TotalCitas = citas.Count;
            ViewBag.Pendientes = citas.Count(c => c.EstadoNombre == "Pendiente");
            ViewBag.Confirmadas = citas.Count(c => c.EstadoNombre == "Confirmada");

            ViewBag.UltimasCitas = citas
                .OrderByDescending(c => c.FechaCita)
                .Take(3)
                .ToList();

            return View();
        }

        // ============================================================
        // 📅 MIS CITAS
        // ============================================================
        public async Task<IActionResult> MisCitas()
        {
            var citas = await _apiService.GetAsync<List<CitaResumenDTO>>(
                "api/ClienteApi/citas") ?? new List<CitaResumenDTO>();

            citas = citas.OrderByDescending(c => c.FechaCita).ToList();

            return View(citas);
        }

        // ============================================================
        // 📅 RESERVAR VISITA (GET)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Reservar(int id)
        {
            if (id <= 0)
                return RedirectToAction("Index", "Propiedades");

            var propiedad = await _apiService.GetAsync<PropiedadDetalleDTO>(
                $"api/PropiedadesPublicas/{id}");

            if (propiedad == null) return NotFound();

            var model = new ReservaViewModel
            {
                IdPropiedad = propiedad.Id,
                Fecha = DateTime.Today.AddDays(1),
                HoraInicio = new TimeSpan(9, 0, 0),
                Tipo = propiedad.Tipo,
                Zona = propiedad.Zona,
                Direccion = propiedad.Direccion,
                Precio = propiedad.Precio,
                Moneda = propiedad.Moneda,
                UrlImagen = propiedad.Imagenes?.FirstOrDefault()?.UrlImagen
            };

            return View(model);
        }

        // ============================================================
        // 📅 RESERVAR VISITA (POST)
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reservar(ReservaViewModel model)
        {
            if (model.IdPropiedad <= 0)
                return RedirectToAction("Index", "Propiedades");

            if (model.Fecha < DateTime.Today)
                ModelState.AddModelError("", "La fecha debe ser de hoy en adelante.");

            if (model.HoraInicio <= TimeSpan.Zero)
                ModelState.AddModelError("", "Indica una hora válida para la visita.");

            if (model.HoraInicio >= new TimeSpan(23, 0, 0))
                ModelState.AddModelError("", "La hora debe ser antes de las 23:00.");

            if (!ModelState.IsValid)
            {
                await CargarDatosPropiedad(model);
                return View(model);
            }

            var resultado = await _apiService.PostAsync<object>("api/ClienteApi/citas", new
            {
                model.IdPropiedad,
                model.Fecha,
                model.HoraInicio,
                HoraFin = model.HoraInicio.Add(TimeSpan.FromHours(1)),
                model.Observaciones
            });

            if (resultado == null)
            {
                ModelState.AddModelError("", "No se pudo reservar la visita. Intenta con otro horario.");
                await CargarDatosPropiedad(model);
                return View(model);
            }

            TempData["MensajeExito"] = "¡Visita reservada! El agente se contactará contigo.";
            return RedirectToAction("MisCitas");
        }

        // ============================================================
        // ❌ CANCELAR CITA
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id)
        {
            var exito = await _apiService.PutAsync($"api/ClienteApi/citas/{id}/cancelar", new { });

            if (exito)
                TempData["MensajeExito"] = "Cita cancelada correctamente.";
            else
                TempData["MensajeError"] = "No se pudo cancelar la cita.";

            return RedirectToAction("MisCitas");
        }

        // ============================================================
        // MÉTODO AUXILIAR: rellenar datos de la propiedad en el modelo
        // ============================================================
        private async Task CargarDatosPropiedad(ReservaViewModel model)
        {
            var propiedad = await _apiService.GetAsync<PropiedadDetalleDTO>(
                $"api/PropiedadesPublicas/{model.IdPropiedad}");

            if (propiedad == null) return;

            model.Tipo = propiedad.Tipo;
            model.Zona = propiedad.Zona;
            model.Direccion = propiedad.Direccion;
            model.Precio = propiedad.Precio;
            model.Moneda = propiedad.Moneda;
            model.UrlImagen = propiedad.Imagenes?.FirstOrDefault()?.UrlImagen;
        }
    }
}