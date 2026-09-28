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
            Console.WriteLine($"🔍 [Index] UsuarioId: {usuarioId}");

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
            Console.WriteLine($"🔍 [MisPropiedades] UsuarioId: {usuarioId}");

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
            Console.WriteLine($"🔍 [CrearPropiedad GET] IdAgente: {model.IdAgente}");
            return View(model);
        }

        // ============================================================
        // CREAR PROPIEDAD (POST)
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearPropiedad(PropiedadCreateViewModel model, List<IFormFile> imagenes)
        {
            Console.WriteLine("=================================================");
            Console.WriteLine("🚀 [CrearPropiedad POST] INICIANDO");
            Console.WriteLine($"🔍 ModelState.IsValid: {ModelState.IsValid}");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("❌ ModelState NO es válido:");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"   - {error.ErrorMessage}");
                }
                return View(model);
            }

            model.IdAgente = ObtenerUsuarioId();
            Console.WriteLine($"🔍 IdAgente obtenido: {model.IdAgente}");
            Console.WriteLine($"🔍 Tipo: {model.Tipo}");
            Console.WriteLine($"🔍 Precio: {model.Precio}");
            Console.WriteLine($"🔍 Zona: {model.Zona}");
            Console.WriteLine($"🔍 Dirección: {model.Direccion}");
            Console.WriteLine($"🔍 Imágenes: {imagenes?.Count ?? 0}");

            // Crear el multipart form data
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
                    Console.WriteLine($"📎 Agregando imagen: {file.FileName} ({file.Length} bytes)");
                    var streamContent = new StreamContent(file.OpenReadStream());
                    streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                    content.Add(streamContent, "files", file.FileName);
                }
            }
            else
            {
                Console.WriteLine("⚠️ No se enviaron imágenes");
            }

            Console.WriteLine("📤 Enviando POST a la API...");
            var resultado = await _apiService.PostFormDataAsync<object>("api/PropiedadesApi", content);

            Console.WriteLine($"📥 Resultado: {(resultado == null ? "NULL (error)" : "OK")}");

            if (resultado == null)
            {
                ModelState.AddModelError("", "❌ Error al crear la propiedad. Revisa la ventana Salida de Visual Studio.");
                return View(model);
            }

            Console.WriteLine("✅ Propiedad creada exitosamente");
            Console.WriteLine("=================================================");

            TempData["MensajeExito"] = "Propiedad creada exitosamente";
            return RedirectToAction("MisPropiedades");
        }

        // ============================================================
        // DETALLE DE PROPIEDAD
        // ============================================================
        public async Task<IActionResult> DetallePropiedad(int id)
        {
            Console.WriteLine($"🔍 [DetallePropiedad] Id: {id}");

            var propiedad = await _apiService.GetAsync<PropiedadDetalleDTO>($"api/PropiedadesApi/{id}");
            if (propiedad == null) return NotFound();

            return View(propiedad);
        }

        // ============================================================
        // EDITAR PROPIEDAD (GET)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> EditarPropiedad(int id)
        {
            Console.WriteLine($"🔍 [EditarPropiedad GET] Id: {id}");

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
            Console.WriteLine($"🚀 [EditarPropiedad POST] Id: {model.Id}");

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
                    IdAgente = model.IdAgente
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
            Console.WriteLine($"🚀 [EliminarPropiedad] Id: {id}");

            var resultado = await _apiService.DeleteAsync($"api/PropiedadesApi/{id}");

            if (resultado)
                TempData["MensajeExito"] = "Propiedad eliminada exitosamente";
            else
                TempData["MensajeError"] = "Error al eliminar la propiedad.";

            return RedirectToAction("MisPropiedades");
        }

        // ============================================================
        // MÉTODO AUXILIAR: Obtener el ID del usuario logueado
        // ============================================================
        private int ObtenerUsuarioId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            var usuarioId = claim != null ? int.Parse(claim.Value) : 0;
            Console.WriteLine($"🔍 [ObtenerUsuarioId] Claim: {claim?.Value ?? "NULL"} → UsuarioId: {usuarioId}");
            return usuarioId;
        }
    }
}