using InmobiliariaMVC.Models;
using InmobiliariaMVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InmobiliariaMVC.Controllers
{
    [Authorize(Roles = "Agente")]
    public class AgenteController : Controller
    {
        private readonly ApiService _apiService;

        public AgenteController(ApiService apiService)
        {
            _apiService = apiService;
        }

        // ============================================================
        // DASHBOARD DEL AGENTE
        // ============================================================
        public async Task<IActionResult> Index()
        {
            var usuarioId = ObtenerUsuarioId();

            var propiedades = await _apiService.GetAsync<List<PropiedadResumenDTO>>(
                $"api/PropiedadesApi/agente/{usuarioId}");

            if (propiedades == null)
                propiedades = new List<PropiedadResumenDTO>();

            ViewBag.TotalPropiedades = propiedades.Count;
            ViewBag.Disponibles = propiedades.Count(p => p.Estado == "Disponible");
            ViewBag.Vendidas = propiedades.Count(p => p.Estado == "Vendido");
            ViewBag.Alquiladas = propiedades.Count(p => p.Estado == "Alquilado");

            decimal comisionTotal = propiedades
                .Where(p => p.ComisionAgentePorcentaje.HasValue)
                .Sum(p => p.Precio * (p.ComisionAgentePorcentaje ?? 0) / 100);

            ViewBag.ComisionTotal = comisionTotal;

            return View();
        }

        // ============================================================
        // MIS PROPIEDADES
        // ============================================================
        public async Task<IActionResult> MisPropiedades()
        {
            var usuarioId = ObtenerUsuarioId();

            var propiedades = await _apiService.GetAsync<List<PropiedadResumenDTO>>(
                $"api/PropiedadesApi/agente/{usuarioId}");

            return View(propiedades ?? new List<PropiedadResumenDTO>());
        }

        // ============================================================
        // CREAR PROPIEDAD (GET)
        // ============================================================
        [HttpGet]
        public IActionResult CrearPropiedad()
        {
            var model = new PropiedadCreateViewModel
            {
                IdAgente = ObtenerUsuarioId()
            };
            return View(model);
        }

        // ============================================================
        // CREAR PROPIEDAD (POST)
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearPropiedad(PropiedadCreateViewModel model, List<IFormFile> imagenes)
        {
            if (!ModelState.IsValid) return View(model);

            model.IdAgente = ObtenerUsuarioId();

            var content = new MultipartFormDataContent();
            content.Add(new StringContent(model.Tipo ?? ""), "Tipo");
            content.Add(new StringContent(model.Precio.ToString()), "Precio");
            content.Add(new StringContent(model.Moneda ?? "USD"), "Moneda");
            content.Add(new StringContent(model.Zona ?? ""), "Zona");
            content.Add(new StringContent(model.Direccion ?? ""), "Direccion");
            content.Add(new StringContent(model.Descripcion ?? ""), "Descripcion");
            content.Add(new StringContent(model.Habitaciones.ToString()), "Habitaciones");
            content.Add(new StringContent(model.Banos.ToString()), "Banos");
            content.Add(new StringContent(model.SuperficieM2?.ToString() ?? ""), "SuperficieM2");
            content.Add(new StringContent(model.IdAgente.ToString()), "IdAgente");

            if (imagenes != null && imagenes.Any())
            {
                foreach (var file in imagenes)
                {
                    var streamContent = new StreamContent(file.OpenReadStream());
                    streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                    content.Add(streamContent, "files", file.FileName);
                }
            }

            var resultado = await _apiService.PostFormDataAsync<object>("api/PropiedadesApi", content);

            if (resultado == null)
            {
                ModelState.AddModelError("", "Error al crear la propiedad.");
                return View(model);
            }

            TempData["MensajeExito"] = "Propiedad creada exitosamente";
            return RedirectToAction("MisPropiedades");
        }

        // ============================================================
        // EDITAR PROPIEDAD (GET)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> EditarPropiedad(int id)
        {
            var propiedad = await _apiService.GetAsync<PropiedadDetalleDTO>($"api/PropiedadesApi/{id}");
            if (propiedad == null) return NotFound();

            var model = new PropiedadCreateViewModel
            {
                Id = propiedad.Id,
                Tipo = propiedad.Tipo,
                Precio = propiedad.Precio,
                Moneda = propiedad.Moneda,
                Zona = propiedad.Zona,
                Direccion = propiedad.Direccion,
                Descripcion = propiedad.Descripcion,
                Habitaciones = propiedad.Habitaciones,
                Banos = propiedad.Banos,
                SuperficieM2 = propiedad.SuperficieM2,
                IdAgente = propiedad.IdAgente
            };

            return View(model);
        }

        // ============================================================
        // EDITAR PROPIEDAD (POST)
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarPropiedad(PropiedadCreateViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            if (!model.Id.HasValue) return NotFound();

            var resultado = await _apiService.PutAsync(
                $"api/PropiedadesApi/{model.Id}",
                new
                {
                    Tipo = model.Tipo,
                    Precio = model.Precio,
                    Moneda = model.Moneda,
                    Zona = model.Zona,
                    Direccion = model.Direccion,
                    Descripcion = model.Descripcion,
                    Habitaciones = model.Habitaciones,
                    Banos = model.Banos,
                    SuperficieM2 = model.SuperficieM2,
                    IdAgente = model.IdAgente,
                    Imagenes = new List<object>()
                });

            if (!resultado)
            {
                ModelState.AddModelError("", "Error al actualizar la propiedad.");
                return View(model);
            }

            TempData["MensajeExito"] = "Propiedad actualizada exitosamente";
            return RedirectToAction("MisPropiedades");
        }

        // ============================================================
        // ELIMINAR PROPIEDAD (POST)
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarPropiedad(int id)
        {
            var resultado = await _apiService.DeleteAsync($"api/PropiedadesApi/{id}");

            if (resultado)
                TempData["MensajeExito"] = "Propiedad eliminada exitosamente";
            else
                TempData["MensajeError"] = "Error al eliminar la propiedad.";

            return RedirectToAction("MisPropiedades");
        }

        // ============================================================
        // MÉTODO AUXILIAR
        // ============================================================
        private int ObtenerUsuarioId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null ? int.Parse(claim.Value) : 0;
        }
    }
}