using InmobiliariaMVC.Models;
using InmobiliariaMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace InmobiliariaMVC.Controllers
{
    // ⚠️ SIN [Authorize] → catálogo público, cualquiera puede verlo
    public class PropiedadesController : Controller
    {
        private readonly ApiService _apiService;

        public PropiedadesController(ApiService apiService)
        {
            _apiService = apiService;
        }

        // ============================================================
        // 📋 CATÁLOGO — con filtros por zona, tipo y precio
        // ============================================================
        public async Task<IActionResult> Index(
            string? tipo,
            string? zona,
            decimal? precioMin,
            decimal? precioMax,
            string? orden)
        {
            var todas = await _apiService.GetAsync<List<PropiedadResumenDTO>>(
                "api/PropiedadesPublicas") ?? new List<PropiedadResumenDTO>();

            // Listas para los filtros (sobre el catálogo completo)
            ViewBag.Zonas = todas
                .Select(p => p.Zona)
                .Where(z => !string.IsNullOrEmpty(z))
                .Distinct()
                .OrderBy(z => z)
                .ToList();

            ViewBag.Tipos = todas
                .Select(p => p.Tipo)
                .Where(t => !string.IsNullOrEmpty(t))
                .Distinct()
                .OrderBy(t => t)
                .ToList();

            var propiedades = todas.AsEnumerable();

            if (!string.IsNullOrEmpty(tipo))
                propiedades = propiedades.Where(p =>
                    p.Tipo.Equals(tipo, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(zona))
                propiedades = propiedades.Where(p =>
                    (p.Zona ?? "").Equals(zona, StringComparison.OrdinalIgnoreCase));

            if (precioMin.HasValue)
                propiedades = propiedades.Where(p => p.Precio >= precioMin.Value);

            if (precioMax.HasValue)
                propiedades = propiedades.Where(p => p.Precio <= precioMax.Value);

            var resultado = propiedades.ToList();

            resultado = orden switch
            {
                "precio_asc" => resultado.OrderBy(p => p.Precio).ToList(),
                "precio_desc" => resultado.OrderByDescending(p => p.Precio).ToList(),
                "recientes" => resultado.OrderByDescending(p => p.FechaPublicacion).ToList(),
                _ => resultado
            };

            // Guardar filtros para que la vista los muestre seleccionados
            ViewBag.Tipo = tipo;
            ViewBag.Zona = zona;
            ViewBag.PrecioMin = precioMin;
            ViewBag.PrecioMax = precioMax;
            ViewBag.Orden = orden;
            ViewBag.Total = resultado.Count;

            return View(resultado);
        }

        // ============================================================
        // 🔍 DETALLE — con galería completa
        // ============================================================
        public async Task<IActionResult> Detalle(int id)
        {
            var propiedad = await _apiService.GetAsync<PropiedadDetalleDTO>(
                $"api/PropiedadesPublicas/{id}");

            if (propiedad == null) return NotFound();

            return View(propiedad);
        }
    }
}