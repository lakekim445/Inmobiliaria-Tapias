using Microsoft.AspNetCore.Mvc;
using TapiaSolution.Shared.DTOs;
using TapiaSolution.Web.Services;

namespace TapiaSolution.Web.Controllers
{
    // ⚠️ SIN [Authorize] → catálogo público, cualquiera puede verlo
    public class PropiedadesController : Controller
    {
        private readonly ApiClient _api;
        public PropiedadesController(ApiClient api) => _api = api;

        // 📋 CATÁLOGO — con filtros por zona, tipo y precio
        public async Task<IActionResult> Index(
            string? tipo,
            string? zona,
            decimal? precioMin,
            decimal? precioMax,
            string? orden)
        {
            // Construir query string hacia la API
            var query = new List<string>();
            if (!string.IsNullOrEmpty(tipo)) query.Add($"tipo={tipo}");
            if (!string.IsNullOrEmpty(zona)) query.Add($"zona={Uri.EscapeDataString(zona)}");
            if (precioMin.HasValue) query.Add($"precioMin={precioMin}");
            if (precioMax.HasValue) query.Add($"precioMax={precioMax}");

            var url = "Propiedades" + (query.Any() ? "?" + string.Join("&", query) : "");
            var propiedades = await _api.GetAsync<List<PropiedadDto>>(url) ?? new();

            // Ordenamiento local (simple)
            propiedades = orden switch
            {
                "precio_asc" => propiedades.OrderBy(p => p.Precio).ToList(),
                "precio_desc" => propiedades.OrderByDescending(p => p.Precio).ToList(),
                "recientes" => propiedades.OrderByDescending(p => p.Id).ToList(),
                _ => propiedades
            };

            // Guardar filtros para que la vista los muestre seleccionados
            ViewBag.Tipo = tipo;
            ViewBag.Zona = zona;
            ViewBag.PrecioMin = precioMin;
            ViewBag.PrecioMax = precioMax;
            ViewBag.Orden = orden;
            ViewBag.Total = propiedades.Count;

            return View(propiedades);
        }

        // 🔍 DETALLE — con galería completa
        public async Task<IActionResult> Detalle(int id)
        {
            var propiedad = await _api.GetAsync<PropiedadDto>($"Propiedades/{id}");
            if (propiedad == null) return NotFound();

            // Guardar si el usuario está logueado (para mostrar botón reservar)
            ViewBag.Logueado = !string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken"));
            ViewBag.Rol = HttpContext.Session.GetString("Rol");

            return View(propiedad);
        }
    }
}