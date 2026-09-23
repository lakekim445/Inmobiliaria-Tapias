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
    [Authorize(Roles = "Admin")]
    public class AdminApiController : ControllerBase
    {
        private readonly InmobiliariaContext _context;

        public AdminApiController(InmobiliariaContext context)
        {
            _context = context;
        }

        // ============================================================
        // GET: /api/admin/dashboard
        // ============================================================
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var propiedades = await _context.Propiedades.ToListAsync();

            var comisiones = propiedades
                .Where(p => p.ComisionEmpresaPorcentaje.HasValue && p.ComisionAgentePorcentaje.HasValue)
                .ToList();

            decimal comisionEmpresaTotal = comisiones
                .Sum(p => p.Precio * (p.ComisionEmpresaPorcentaje ?? 0) / 100);

            decimal comisionAgenteTotal = comisiones
                .Sum(p => p.Precio * (p.ComisionAgentePorcentaje ?? 0) / 100);

            var dto = new AdminDashboardDTO
            {
                TotalUsuarios = await _context.Usuarios.CountAsync(),
                TotalAgentes = await _context.Usuarios.CountAsync(u => u.IdRol == 2),
                TotalClientes = await _context.Clientes.CountAsync(),

                TotalPropiedades = propiedades.Count,
                PropiedadesDisponibles = propiedades.Count(p => p.Estado == "Disponible"),
                PropiedadesReservadas = propiedades.Count(p => p.Estado == "Reservado"),
                PropiedadesVendidas = propiedades.Count(p => p.Estado == "Vendido"),
                PropiedadesAlquiladas = propiedades.Count(p => p.Estado == "Alquilado"),
                PropiedadesAnticretico = propiedades.Count(p => p.Estado == "Anticretico"),

                TotalCitas = await _context.Citas.CountAsync(),
                CitasPendientes = await _context.Citas.CountAsync(c => c.IdEstadoCita == 1),
                CitasConfirmadas = await _context.Citas.CountAsync(c => c.IdEstadoCita == 2),
                CitasCompletadas = await _context.Citas.CountAsync(c => c.IdEstadoCita == 4),
                CitasCanceladas = await _context.Citas.CountAsync(c => c.IdEstadoCita == 3),

                ComisionTotalEmpresa = comisionEmpresaTotal,
                ComisionTotalAgentes = comisionAgenteTotal,
                GananciaNetaEmpresa = comisionEmpresaTotal - comisionAgenteTotal
            };

            return Ok(dto);
        }

        // ============================================================
        // GET: /api/admin/agentes
        // ============================================================
        [HttpGet("agentes")]
        public async Task<IActionResult> GetAgentes()
        {
            var agentes = await _context.Usuarios
                .Where(u => u.IdRol == 2)
                .ToListAsync();

            var propiedades = await _context.Propiedades.ToListAsync();

            var resultado = agentes.Select(a =>
            {
                var misPropiedades = propiedades.Where(p => p.IdAgente == a.Id).ToList();
                var cerradas = misPropiedades.Where(p => p.ComisionAgentePorcentaje.HasValue).ToList();

                return new AgenteResumenDTO
                {
                    Id = a.Id,
                    NombreCompleto = a.NombreCompleto ?? "",
                    Email = a.Email ?? "",
                    Telefono = a.Telefono ?? "",
                    Activo = a.Activo,
                    FechaRegistro = a.FechaRegistro,

                    TotalPropiedades = misPropiedades.Count,
                    PropiedadesDisponibles = misPropiedades.Count(p => p.Estado == "Disponible"),
                    PropiedadesVendidas = misPropiedades.Count(p => p.Estado == "Vendido"),
                    PropiedadesAlquiladas = misPropiedades.Count(p => p.Estado == "Alquilado"),
                    PropiedadesAnticretico = misPropiedades.Count(p => p.Estado == "Anticretico"),

                    ComisionTotal = cerradas.Sum(p => p.Precio * (p.ComisionAgentePorcentaje ?? 0) / 100)
                };
            }).ToList();

            return Ok(resultado);
        }

        // ============================================================
        // GET: /api/admin/propiedades
        // ============================================================
        [HttpGet("propiedades")]
        public async Task<IActionResult> GetPropiedades()
        {
            var propiedades = await _context.Propiedades
                .Include(p => p.Agente)
                .Include(p => p.Imagenes)
                .OrderByDescending(p => p.FechaPublicacion)
                .ToListAsync();

            var resultado = propiedades.Select(p => new PropiedadResumenDTO
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

                UrlImagenPrincipal = p.Imagenes?
                    .FirstOrDefault(i => i.EsPrincipal)?.UrlImagen
                    ?? p.Imagenes?.FirstOrDefault()?.UrlImagen
            }).ToList();

            return Ok(resultado);
        }

        // ============================================================
        // GET: /api/admin/propiedades/{id}
        // ============================================================
        [HttpGet("propiedades/{id}")]
        public async Task<IActionResult> GetPropiedad(int id)
        {
            var p = await _context.Propiedades
                .Include(p => p.Agente)
                .Include(p => p.Imagenes)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (p == null) return NotFound();

            return Ok(p);
        }

        // ============================================================
        // GET: /api/admin/clientes
        // ============================================================
        [HttpGet("clientes")]
        public async Task<IActionResult> GetClientes()
        {
            var clientes = await _context.Clientes
                .Include(c => c.EstadoProspecto)
                .ToListAsync();

            return Ok(clientes);
        }

        // ============================================================
        // GET: /api/admin/citas
        // ============================================================
        [HttpGet("citas")]
        public async Task<IActionResult> GetCitas()
        {
            var citas = await _context.Citas
                .Include(c => c.Cliente)
                .Include(c => c.Propiedad)
                .Include(c => c.Agente)
                .Include(c => c.EstadoCita)
                .OrderByDescending(c => c.FechaCita)
                .ToListAsync();

            return Ok(citas);
        }

        // ============================================================
        // GET: /api/admin/comisiones
        // ============================================================
        [HttpGet("comisiones")]
        public async Task<IActionResult> GetComisiones()
        {
            var propiedadesCerradas = await _context.Propiedades
                .Include(p => p.Agente)
                .Where(p => p.ComisionEmpresaPorcentaje.HasValue
                         && p.ComisionAgentePorcentaje.HasValue)
                .OrderByDescending(p => p.FechaCierre)
                .ToListAsync();

            var resultado = propiedadesCerradas.Select(p =>
            {
                decimal comisionEmpresa = p.Precio * (p.ComisionEmpresaPorcentaje ?? 0) / 100;
                decimal comisionAgente = p.Precio * (p.ComisionAgentePorcentaje ?? 0) / 100;

                return new ComisionDTO
                {
                    IdPropiedad = p.Id,
                    NombrePropiedad = $"{p.Tipo} en {p.Zona}",
                    Tipo = p.Tipo ?? "",
                    Zona = p.Zona ?? "",
                    TipoOperacion = p.TipoOperacion ?? "",
                    Precio = p.Precio,
                    Moneda = p.Moneda ?? "USD",
                    FechaCierre = p.FechaCierre,

                    IdAgente = p.IdAgente,
                    NombreAgente = p.Agente?.NombreCompleto ?? "",

                    ComisionEmpresaPorcentaje = p.ComisionEmpresaPorcentaje ?? 0,
                    ComisionEmpresaMonto = comisionEmpresa,

                    ComisionAgentePorcentaje = p.ComisionAgentePorcentaje ?? 0,
                    ComisionAgenteMonto = comisionAgente,

                    GananciaNetaEmpresa = comisionEmpresa - comisionAgente
                };
            }).ToList();

            return Ok(resultado);
        }

        // ============================================================
        // POST: /api/admin/propiedades/{id}/cerrar-operacion
        // ============================================================
        [HttpPost("propiedades/{id}/cerrar-operacion")]
        public async Task<IActionResult> CerrarOperacion(int id, [FromBody] CerrarOperacionDTO dto)
        {
            var propiedad = await _context.Propiedades.FindAsync(id);
            if (propiedad == null) return NotFound();

            decimal empresaPorcentaje;
            decimal agentePorcentaje;
            string nuevoEstado;

            switch (dto.TipoOperacion)
            {
                case "Venta":
                    empresaPorcentaje = 5.00m;
                    agentePorcentaje = 1.00m;
                    nuevoEstado = "Vendido";
                    break;
                case "Alquiler":
                    empresaPorcentaje = 5.00m;
                    agentePorcentaje = 1.00m;
                    nuevoEstado = "Alquilado";
                    break;
                case "Anticretico":
                    empresaPorcentaje = 3.00m;
                    agentePorcentaje = 0.50m;
                    nuevoEstado = "Anticretico";
                    break;
                default:
                    return BadRequest(new { mensaje = "Tipo de operación inválido" });
            }

            propiedad.TipoOperacion = dto.TipoOperacion;
            propiedad.Precio = dto.PrecioFinal;
            propiedad.ComisionEmpresaPorcentaje = empresaPorcentaje;
            propiedad.ComisionAgentePorcentaje = agentePorcentaje;
            propiedad.FechaCierre = DateTime.UtcNow;
            propiedad.Estado = nuevoEstado;

            await _context.SaveChangesAsync();

            decimal comisionEmpresa = propiedad.Precio * empresaPorcentaje / 100;
            decimal comisionAgente = propiedad.Precio * agentePorcentaje / 100;
            decimal gananciaNeta = comisionEmpresa - comisionAgente;

            return Ok(new
            {
                mensaje = "Operación cerrada exitosamente",
                comisionEmpresa,
                comisionAgente,
                gananciaNeta
            });
        }
    }
}