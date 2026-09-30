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

            var noLeidas = await _apiService.GetAsync<int>("api/AgenteApi/notificaciones/noleidas");
            ViewBag.NotifNoLeidas = noLeidas;

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
            content.Add(new StringContent(model.TipoOperacion ?? "Venta"), "TipoOperacion");
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
        // DETALLE DE PROPIEDAD
        // ============================================================
        public async Task<IActionResult> DetallePropiedad(int id)
        {
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
            var propiedad = await _apiService.GetAsync<PropiedadDetalleDTO>($"api/PropiedadesApi/{id}");
            if (propiedad == null) return NotFound();

            var model = new PropiedadEditViewModel
            {
                Id = propiedad.Id,
                Tipo = propiedad.Tipo,
                TipoOperacion = propiedad.TipoOperacion ?? "Venta",
                Precio = propiedad.Precio,
                Moneda = propiedad.Moneda,
                Zona = propiedad.Zona,
                Direccion = propiedad.Direccion,
                Descripcion = propiedad.Descripcion,
                Habitaciones = propiedad.Habitaciones,
                Banos = propiedad.Banos,
                SuperficieM2 = propiedad.SuperficieM2,
                IdAgente = propiedad.IdAgente,
                Imagenes = propiedad.Imagenes ?? new List<ImagenPropiedadDTO>()
            };

            return View(model);
        }

        // ============================================================
        // EDITAR PROPIEDAD (POST)
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarPropiedad(PropiedadEditViewModel model, List<IFormFile> nuevasImagenes)
        {
            if (!ModelState.IsValid) return View(model);
            if (model.Id == 0) return NotFound();

            // 1. Actualizar los campos de texto
            var resultado = await _apiService.PutAsync(
                $"api/PropiedadesApi/{model.Id}",
                new
                {
                    Tipo = model.Tipo,
                    TipoOperacion = model.TipoOperacion,
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

            // 2. Subir las nuevas imágenes (si las hay)
            if (nuevasImagenes != null && nuevasImagenes.Any())
            {
                foreach (var file in nuevasImagenes)
                {
                    if (file.Length > 0)
                    {
                        var content = new MultipartFormDataContent();
                        var streamContent = new StreamContent(file.OpenReadStream());
                        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                        content.Add(streamContent, "file", file.FileName);

                        await _apiService.PostFormDataAsync<object>(
                            $"api/PropiedadesApi/{model.Id}/imagenes",
                            content);
                    }
                }
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
        // MIS CITAS (visitas asignadas al agente)
        // ============================================================
        public async Task<IActionResult> Citas()
        {
            var citas = await _apiService.GetAsync<List<CitaResumenDTO>>("api/AgenteApi/citas");
            if (citas == null)
                citas = new List<CitaResumenDTO>();

            ViewBag.Pendientes = citas.Count(c => c.EstadoNombre == "Pendiente");
            ViewBag.Confirmadas = citas.Count(c => c.EstadoNombre == "Confirmada");
            ViewBag.Canceladas = citas.Count(c => c.EstadoNombre == "Cancelada");

            return View(citas.OrderByDescending(c => c.FechaCita).ToList());
        }

        // ============================================================
        // NUEVA CITA (GET) - el agente agenda la visita manualmente
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> NuevaCita()
        {
            var model = new NuevaCitaViewModel();
            await CargarListasCita(model);
            return View(model);
        }

        // ============================================================
        // NUEVA CITA (POST)
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NuevaCita(NuevaCitaViewModel model)
        {
            if (model.Modo == "nuevo")
            {
                if (string.IsNullOrWhiteSpace(model.NombreClienteNuevo))
                    ModelState.AddModelError("", "Ingresa el nombre del cliente.");
                model.IdCliente = 0;
            }
            else if (model.IdCliente <= 0)
            {
                ModelState.AddModelError("", "Selecciona un cliente o marca la opción 'cliente sin cuenta'.");
            }

            if (model.IdPropiedad <= 0)
                ModelState.AddModelError("", "Selecciona una propiedad.");
            if (model.Fecha.Date < DateTime.Today)
                ModelState.AddModelError("", "La fecha debe ser de hoy en adelante.");
            if (model.HoraInicio <= TimeSpan.Zero)
                ModelState.AddModelError("", "Indica una hora válida para la visita.");

            if (!ModelState.IsValid)
            {
                await CargarListasCita(model);
                return View(model);
            }

            var (resultado, error) = await _apiService.PostConErrorAsync<CitaCreadaDTO>(
                "api/AgenteApi/citas",
                new
                {
                    model.IdCliente,
                    model.IdPropiedad,
                    model.Fecha,
                    model.HoraInicio,
                    HoraFin = model.HoraInicio.Add(TimeSpan.FromHours(1)),
                    model.Observaciones,
                    NombreClienteNuevo = model.Modo == "nuevo" ? model.NombreClienteNuevo : null,
                    TelefonoClienteNuevo = model.Modo == "nuevo" ? model.TelefonoClienteNuevo : null,
                    EmailClienteNuevo = model.Modo == "nuevo" ? model.EmailClienteNuevo : null
                });

            if (resultado == null || string.IsNullOrWhiteSpace(resultado.Mensaje))
            {
                ModelState.AddModelError("", error ?? "No se pudo registrar la cita. Revisa el horario.");
                await CargarListasCita(model);
                return View(model);
            }

            TempData["MensajeExito"] = resultado.Mensaje;
            return RedirectToAction("Citas");
        }

        // ============================================================
        // CANCELAR CITA (POST)
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarCita(int id)
        {
            var resultado = await _apiService.PutAsync($"api/AgenteApi/citas/{id}/cancelar", new { });

            if (resultado)
                TempData["MensajeExito"] = "Cita cancelada correctamente.";
            else
                TempData["MensajeError"] = "No se pudo cancelar la cita.";

            return RedirectToAction("Citas");
        }

        // ============================================================
        // CONFIRMAR CITA (POST)
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarCita(int id)
        {
            var resultado = await _apiService.PutAsync($"api/AgenteApi/citas/{id}/confirmar", new { });

            if (resultado)
                TempData["MensajeExito"] = "Cita confirmada correctamente.";
            else
                TempData["MensajeError"] = "No se pudo confirmar la cita.";

            return RedirectToAction("Citas");
        }

        // ============================================================
        // COMPLETAR CITA (POST)
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompletarCita(int id)
        {
            var resultado = await _apiService.PutAsync($"api/AgenteApi/citas/{id}/completar", new { });

            if (resultado)
                TempData["MensajeExito"] = "Visita registrada como completada.";
            else
                TempData["MensajeError"] = "No se pudo completar la visita.";

            return RedirectToAction("Citas");
        }

        // ============================================================
        // NOTIFICACIONES DEL AGENTE
        // ============================================================
        public async Task<IActionResult> Notificaciones()
        {
            var lista = await _apiService.GetAsync<List<NotificacionResumenDTO>>("api/AgenteApi/notificaciones");
            return View(lista ?? new List<NotificacionResumenDTO>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarLeida(int id)
        {
            await _apiService.PutAsync($"api/AgenteApi/notificaciones/{id}/leer", new { });
            return RedirectToAction("Notificaciones");
        }

        // ============================================================
        // MÉTODO AUXILIAR: Cargar listas de clientes y propiedades
        // ============================================================
        private async Task CargarListasCita(NuevaCitaViewModel model)
        {
            var usuarioId = ObtenerUsuarioId();

            var clientes = await _apiService.GetAsync<List<ClienteResumenDTO>>("api/AgenteApi/clientes");
            model.Clientes = clientes ?? new List<ClienteResumenDTO>();

            var propiedades = await _apiService.GetAsync<List<PropiedadResumenDTO>>($"api/PropiedadesApi/agente/{usuarioId}");
            model.Propiedades = propiedades ?? new List<PropiedadResumenDTO>();
        }

        // ============================================================
        // MÉTODO AUXILIAR: Obtener el ID del usuario logueado
        // ============================================================
        private int ObtenerUsuarioId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null ? int.Parse(claim.Value) : 0;
        }
    }
}