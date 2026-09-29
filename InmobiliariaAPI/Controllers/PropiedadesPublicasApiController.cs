using InmobiliariaAPI.Data;
using InmobiliariaAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InmobiliariaAPI.Controllers
{
    [Route("api/PropiedadesPublicas")]
    [ApiController]
    [AllowAnonymous]
    public class PropiedadesPublicasApiController : ControllerBase
    {
        private readonly InmobiliariaContext _context;

        public PropiedadesPublicasApiController(InmobiliariaContext context)
        {
            _context = context;
        }

        // ============================================================
        // GET: /api/propiedadespublicas (catálogo público)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> GetCatalogo()
        {
            var propiedades = await _context.Propiedades
                .Include(p => p.Agente)
                .Include(p => p.Imagenes)
                .Where(p => p.Estado == "Disponible")
                .OrderByDescending(p => p.FechaPublicacion)
                .ToListAsync();

            var resultado = propiedades.Select(p =>
            {
                var imagenPrincipal = p.Imagenes?.OrderByDescending(i => i.EsPrincipal).FirstOrDefault();
                return new PropiedadResumenDTO
                {
                    Id = p.Id,
                    Tipo = p.Tipo ?? "",
                    Precio = p.Precio,
                    Moneda = p.Moneda ?? "USD",
                    Zona = p.Zona ?? "",
                    Direccion = p.Direccion ?? "",
                    Estado = p.Estado ?? "",
                    Habitaciones = p.Habitaciones,
                    Banos = p.Banos,
                    SuperficieM2 = p.SuperficieM2,
                    FechaPublicacion = p.FechaPublicacion,
                    IdAgente = p.IdAgente,
                    NombreAgente = p.Agente?.NombreCompleto ?? "",
                    TipoOperacion = p.TipoOperacion,
                    FechaCierre = p.FechaCierre,
                    ComisionEmpresaPorcentaje = p.ComisionEmpresaPorcentaje,
                    ComisionAgentePorcentaje = p.ComisionAgentePorcentaje,
                    UrlImagenPrincipal = imagenPrincipal?.UrlImagen
                };
            }).ToList();

            return Ok(resultado);
        }

        // ============================================================
        // GET: /api/propiedadespublicas/{id} (detalle público)
        // ============================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetalle(int id)
        {
            var p = await _context.Propiedades
                .Include(p => p.Agente)
                .Include(p => p.Imagenes)
                .FirstOrDefaultAsync(p => p.Id == id && p.Estado == "Disponible");

            if (p == null) return NotFound();

            return Ok(new PropiedadDetalleDTO
            {
                Id = p.Id,
                Tipo = p.Tipo ?? "",
                Precio = p.Precio,
                Moneda = p.Moneda ?? "USD",
                Zona = p.Zona ?? "",
                Direccion = p.Direccion ?? "",
                Descripcion = p.Descripcion,
                Habitaciones = p.Habitaciones,
                Banos = p.Banos,
                SuperficieM2 = p.SuperficieM2,
                Estado = p.Estado ?? "",
                FechaPublicacion = p.FechaPublicacion,
                IdAgente = p.IdAgente,
                NombreAgente = p.Agente?.NombreCompleto ?? "",
                TipoOperacion = p.TipoOperacion,
                FechaCierre = p.FechaCierre,
                ComisionEmpresaPorcentaje = p.ComisionEmpresaPorcentaje,
                ComisionAgentePorcentaje = p.ComisionAgentePorcentaje,
                Imagenes = p.Imagenes?.Select(i => new ImagenPropiedadDTO
                {
                    Id = i.Id,
                    UrlImagen = i.UrlImagen ?? "",
                    Descripcion = i.Descripcion,
                    EsPrincipal = i.EsPrincipal
                }).ToList() ?? new List<ImagenPropiedadDTO>()
            });
        }
    }
}