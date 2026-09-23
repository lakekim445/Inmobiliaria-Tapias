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
    }
}