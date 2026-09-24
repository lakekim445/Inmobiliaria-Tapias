using InmobiliariaAPI.Data;
using InmobiliariaAPI.DTOs;
using InmobiliariaAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InmobiliariaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PropiedadesApiController : ControllerBase
    {
        private readonly InmobiliariaContext _context;

        public PropiedadesApiController(InmobiliariaContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetPropiedades()
        {
            var propiedades = await _context.Propiedades
                .Include(p => p.Agente)
                .Include(p => p.Imagenes)
                .Where(p => p.Estado == "Disponible")
                .OrderByDescending(p => p.FechaPublicacion)
                .ToListAsync();

            var resultado = propiedades.Select(p => MapToDetalleDTO(p)).ToList();
            return Ok(resultado);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPropiedad(int id)
        {
            var p = await _context.Propiedades
                .Include(p => p.Agente)
                .Include(p => p.Imagenes)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (p == null) return NotFound();

            return Ok(MapToDetalleDTO(p));
        }

        [HttpGet("agente/{id}")]
        public async Task<IActionResult> GetPropiedadesPorAgente(int id)
        {
            var propiedades = await _context.Propiedades
                .Include(p => p.Agente)
                .Include(p => p.Imagenes)
                .Where(p => p.IdAgente == id)
                .OrderByDescending(p => p.FechaPublicacion)
                .ToListAsync();

            var resultado = propiedades.Select(p => MapToDetalleDTO(p)).ToList();
            return Ok(resultado);
        }

        [HttpPost]
        [Authorize(Roles = "Agente,Admin")]
        public async Task<IActionResult> CrearPropiedad([FromBody] PropiedadCreateDTO dto)
        {
            if (string.IsNullOrEmpty(dto.Tipo) ||
                dto.Precio <= 0 ||
                string.IsNullOrEmpty(dto.Zona) ||
                string.IsNullOrEmpty(dto.Direccion))
            {
                return BadRequest(new { mensaje = "Faltan campos obligatorios" });
            }

            var nuevaPropiedad = new Propiedad
            {
                Tipo = dto.Tipo,
                Precio = dto.Precio,
                Moneda = dto.Moneda,
                Zona = dto.Zona,
                Direccion = dto.Direccion,
                Descripcion = dto.Descripcion,
                Habitaciones = dto.Habitaciones,
                Banos = dto.Banos,
                SuperficieM2 = dto.SuperficieM2,
                Estado = "Disponible",
                FechaPublicacion = DateTime.UtcNow,
                IdAgente = dto.IdAgente
            };

            _context.Propiedades.Add(nuevaPropiedad);
            await _context.SaveChangesAsync();

            if (dto.Imagenes != null && dto.Imagenes.Any())
            {
                foreach (var img in dto.Imagenes)
                {
                    _context.ImagenesPropiedad.Add(new ImagenPropiedad
                    {
                        UrlImagen = img.UrlImagen,
                        Descripcion = img.Descripcion,
                        EsPrincipal = img.EsPrincipal,
                        IdPropiedad = nuevaPropiedad.Id
                    });
                }
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                mensaje = "Propiedad creada exitosamente",
                id = nuevaPropiedad.Id
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Agente,Admin")]
        public async Task<IActionResult> EditarPropiedad(int id, [FromBody] PropiedadCreateDTO dto)
        {
            var propiedad = await _context.Propiedades.FindAsync(id);
            if (propiedad == null) return NotFound();

            propiedad.Tipo = dto.Tipo;
            propiedad.Precio = dto.Precio;
            propiedad.Moneda = dto.Moneda;
            propiedad.Zona = dto.Zona;
            propiedad.Direccion = dto.Direccion;
            propiedad.Descripcion = dto.Descripcion;
            propiedad.Habitaciones = dto.Habitaciones;
            propiedad.Banos = dto.Banos;
            propiedad.SuperficieM2 = dto.SuperficieM2;

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Propiedad actualizada exitosamente" });
        }


        [HttpDelete("{id}")]
        [Authorize(Roles = "Agente,Admin")]
        public async Task<IActionResult> EliminarPropiedad(int id)
        {
            var propiedad = await _context.Propiedades
                .Include(p => p.Imagenes)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (propiedad == null) return NotFound();

            if (propiedad.Imagenes != null && propiedad.Imagenes.Any())
                _context.ImagenesPropiedad.RemoveRange(propiedad.Imagenes);

            _context.Propiedades.Remove(propiedad);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Propiedad eliminada exitosamente" });
        }


        [HttpPost("{id}/imagenes")]
        [Authorize(Roles = "Agente,Admin")]
        public async Task<IActionResult> SubirImagen(int id, [FromBody] ImagenPropiedadDTO dto)
        {
            var propiedad = await _context.Propiedades.FindAsync(id);
            if (propiedad == null) return NotFound();

            var imagen = new ImagenPropiedad
            {
                UrlImagen = dto.UrlImagen,
                Descripcion = dto.Descripcion,
                EsPrincipal = dto.EsPrincipal,
                IdPropiedad = id
            };

            _context.ImagenesPropiedad.Add(imagen);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Imagen subida exitosamente", id = imagen.Id });
        }

        private PropiedadDetalleDTO MapToDetalleDTO(Propiedad p)
        {
            return new PropiedadDetalleDTO
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
            };
        }
    }
}