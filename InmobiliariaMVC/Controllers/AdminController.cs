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