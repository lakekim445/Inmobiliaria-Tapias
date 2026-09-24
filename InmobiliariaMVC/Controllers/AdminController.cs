using InmobiliariaMVC.Models;
using InmobiliariaMVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InmobiliariaMVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApiService _apiService;

        public AdminController(ApiService apiService)
        {
            _apiService = apiService;
        }
        public async Task<IActionResult> Index()
        {
            var dashboard = await _apiService.GetAsync<AdminDashboardDTO>("api/AdminApi/dashboard");
            return View(dashboard ?? new AdminDashboardDTO());
        }

        public async Task<IActionResult> Agentes()
        {
            var agentes = await _apiService.GetAsync<List<AgenteResumenDTO>>("api/AdminApi/agentes");
            return View(agentes ?? new List<AgenteResumenDTO>());
        }

        public async Task<IActionResult> DetalleAgente(int id)
        {
            var agente = await _apiService.GetAsync<AgenteDetalleDTO>($"api/AdminApi/agentes/{id}");
            if (agente == null) return NotFound();
            return View(agente);
        }

        [HttpGet]
        public IActionResult CrearAgente()
        {
            return View(new AgenteCreateViewModel());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearAgente(AgenteCreateViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var resultado = await _apiService.PostAsync<object>(
                "api/AdminApi/agentes",
                new
                {
                    NombreCompleto = model.NombreCompleto,
                    Email = model.Email,
                    Telefono = model.Telefono,
                    Password = model.Password
                });

            if (resultado == null)
            {
                ModelState.AddModelError("", "Error al crear el agente. Verifica que el email no esté en uso.");
                return View(model);
            }

            TempData["MensajeExito"] = "Agente creado exitosamente";
            return RedirectToAction("Agentes");
        }

        [HttpGet]
        public async Task<IActionResult> EditarAgente(int id)
        {
            var agente = await _apiService.GetAsync<AgenteDetalleDTO>($"api/AdminApi/agentes/{id}");
            if (agente == null) return NotFound();

            var model = new AgenteUpdateViewModel
            {
                Id = agente.Id,
                NombreCompleto = agente.NombreCompleto,
                Email = agente.Email,
                Telefono = agente.Telefono,
                Activo = agente.Activo
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarAgente(AgenteUpdateViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var resultado = await _apiService.PutAsync(
                $"api/AdminApi/agentes/{model.Id}",
                new
                {
                    NombreCompleto = model.NombreCompleto,
                    Email = model.Email,
                    Telefono = model.Telefono,
                    Activo = model.Activo
                });

            if (!resultado)
            {
                ModelState.AddModelError("", "Error al actualizar el agente.");
                return View(model);
            }

            TempData["MensajeExito"] = "Agente actualizado exitosamente";
            return RedirectToAction("Agentes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarAgente(int id)
        {
            var resultado = await _apiService.DeleteAsync($"api/AdminApi/agentes/{id}");

            if (resultado)
                TempData["MensajeExito"] = "Agente eliminado exitosamente";
            else
                TempData["MensajeError"] = "Error al eliminar. Puede tener propiedades asignadas.";

            return RedirectToAction("Agentes");
        }

        public async Task<IActionResult> Propiedades()
        {
            var propiedades = await _apiService.GetAsync<List<PropiedadResumenDTO>>("api/AdminApi/propiedades");
            return View(propiedades ?? new List<PropiedadResumenDTO>());
        }

        public async Task<IActionResult> Clientes()
        {
            var clientes = await _apiService.GetAsync<List<object>>("api/AdminApi/clientes");
            return View(clientes ?? new List<object>());
        }

        public async Task<IActionResult> Citas()
        {
            var citas = await _apiService.GetAsync<List<object>>("api/AdminApi/citas");
            return View(citas ?? new List<object>());
        }

        public async Task<IActionResult> Comisiones()
        {
            var comisiones = await _apiService.GetAsync<List<ComisionDTO>>("api/AdminApi/comisiones");
            return View(comisiones ?? new List<ComisionDTO>());
        }

        public async Task<IActionResult> PropiedadesPorAgente(int id)
        {
            var propiedades = await _apiService.GetAsync<List<PropiedadResumenDTO>>("api/AdminApi/propiedades");

            if (propiedades == null)
                return View(new List<PropiedadResumenDTO>());

            var filtradas = propiedades.Where(p => p.IdAgente == id).ToList();
            ViewBag.IdAgente = id;

            return View(filtradas);
        }
        // ============================================================
        // PROPIEDADES - DETALLE
        // ============================================================
        public async Task<IActionResult> DetallePropiedad(int id)
        {
            var propiedad = await _apiService.GetAsync<PropiedadResumenDTO>($"api/AdminApi/propiedades/{id}");
            if (propiedad == null) return NotFound();
            return View(propiedad);
        }

        // ============================================================
        // PROPIEDADES - EDITAR (GET)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> EditarPropiedad(int id)
        {
            var propiedad = await _apiService.GetAsync<PropiedadResumenDTO>($"api/AdminApi/propiedades/{id}");
            if (propiedad == null) return NotFound();

            var model = new PropiedadUpdateViewModel
            {
                Id = propiedad.Id,
                Tipo = propiedad.Tipo,
                Precio = propiedad.Precio,
                Moneda = propiedad.Moneda,
                Zona = propiedad.Zona,
                Direccion = propiedad.Direccion,
                Habitaciones = propiedad.Habitaciones,
                Banos = propiedad.Banos,
                SuperficieM2 = propiedad.SuperficieM2,
                Estado = propiedad.Estado
            };

            return View(model);
        }

        // ============================================================
        // PROPIEDADES - EDITAR (POST)
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarPropiedad(PropiedadUpdateViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var resultado = await _apiService.PutAsync(
                $"api/AdminApi/propiedades/{model.Id}",
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
                    Estado = model.Estado
                });

            if (!resultado)
            {
                ModelState.AddModelError("", "Error al actualizar la propiedad.");
                return View(model);
            }

            TempData["MensajeExito"] = "Propiedad actualizada exitosamente";
            return RedirectToAction("Propiedades");
        }

        // ============================================================
        // PROPIEDADES - ELIMINAR
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarPropiedad(int id)
        {
            var resultado = await _apiService.DeleteAsync($"api/AdminApi/propiedades/{id}");

            if (resultado)
                TempData["MensajeExito"] = "Propiedad eliminada exitosamente";
            else
                TempData["MensajeError"] = "Error al eliminar la propiedad.";

            return RedirectToAction("Propiedades");
        }

        // ============================================================
        // PROPIEDADES - CERRAR OPERACIÓN (GET)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> CerrarOperacion(int id)
        {
            var propiedad = await _apiService.GetAsync<PropiedadResumenDTO>($"api/AdminApi/propiedades/{id}");
            if (propiedad == null) return NotFound();

            var model = new CerrarOperacionViewModel
            {
                Id = propiedad.Id,
                NombrePropiedad = $"{propiedad.Tipo} en {propiedad.Zona}",
                PrecioPublicado = propiedad.Precio,
                Moneda = propiedad.Moneda
            };

            return View(model);
        }

        // ============================================================
        // PROPIEDADES - CERRAR OPERACIÓN (POST)
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CerrarOperacion(CerrarOperacionViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var resultado = await _apiService.PostAsync<object>(
                $"api/AdminApi/propiedades/{model.Id}/cerrar-operacion",
                new
                {
                    TipoOperacion = model.TipoOperacion,
                    PrecioFinal = model.PrecioFinal,
                    Observaciones = model.Observaciones
                });

            if (resultado == null)
            {
                ModelState.AddModelError("", "Error al cerrar la operación.");
                return View(model);
            }

            TempData["MensajeExito"] = "Operación cerrada exitosamente";
            return RedirectToAction("Propiedades");
        }
    }
}