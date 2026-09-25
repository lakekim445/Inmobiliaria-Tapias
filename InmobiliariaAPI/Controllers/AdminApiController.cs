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
        // DASHBOARD
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
        // AGENTES - LISTAR
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
        // AGENTES - DETALLE
        // ============================================================
        [HttpGet("agentes/{id}")]
        public async Task<IActionResult> GetAgente(int id)
        {
            var agente = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Id == id && u.IdRol == 2);

            if (agente == null) return NotFound();

            return Ok(new AgenteDetalleDTO
            {
                Id = agente.Id,
                NombreCompleto = agente.NombreCompleto ?? "",
                Email = agente.Email ?? "",
                Telefono = agente.Telefono ?? "",
                Activo = agente.Activo,
                FechaRegistro = agente.FechaRegistro,
                NombreRol = agente.Rol?.NombreRol ?? ""
            });
        }

        // ============================================================
        // AGENTES - CREAR
        // ============================================================
        [HttpPost("agentes")]
        public async Task<IActionResult> CrearAgente([FromBody] AgenteCreateDTO dto)
        {
            if (string.IsNullOrEmpty(dto.NombreCompleto) ||
                string.IsNullOrEmpty(dto.Email) ||
                string.IsNullOrEmpty(dto.Password))
            {
                return BadRequest(new { mensaje = "Todos los campos son obligatorios" });
            }

            var emailExiste = await _context.Usuarios.AnyAsync(u => u.Email == dto.Email);
            if (emailExiste)
                return BadRequest(new { mensaje = "El email ya está registrado" });

            var nuevoAgente = new Usuario
            {
                NombreCompleto = dto.NombreCompleto,
                Email = dto.Email,
                PasswordHash = dto.Password,
                Telefono = dto.Telefono,
                FechaRegistro = DateTime.UtcNow,
                Activo = true,
                IdRol = 2
            };

            _context.Usuarios.Add(nuevoAgente);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Agente creado exitosamente",
                id = nuevoAgente.Id
            });
        }

        // ============================================================
        // AGENTES - EDITAR
        // ============================================================
        [HttpPut("agentes/{id}")]
        public async Task<IActionResult> EditarAgente(int id, [FromBody] AgenteUpdateDTO dto)
        {
            var agente = await _context.Usuarios.FindAsync(id);
            if (agente == null) return NotFound();
            if (agente.IdRol != 2) return BadRequest(new { mensaje = "El usuario no es un agente" });

            var emailEnUso = await _context.Usuarios
                .AnyAsync(u => u.Email == dto.Email && u.Id != id);

            if (emailEnUso)
                return BadRequest(new { mensaje = "El email ya está en uso por otro usuario" });

            agente.NombreCompleto = dto.NombreCompleto;
            agente.Email = dto.Email;
            agente.Telefono = dto.Telefono;
            agente.Activo = dto.Activo;

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Agente actualizado exitosamente" });
        }

        // ============================================================
        // AGENTES - ELIMINAR
        // ============================================================
        [HttpDelete("agentes/{id}")]
        public async Task<IActionResult> EliminarAgente(int id)
        {
            var agente = await _context.Usuarios.FindAsync(id);
            if (agente == null) return NotFound();
            if (agente.IdRol != 2) return BadRequest(new { mensaje = "El usuario no es un agente" });

            var tienePropiedades = await _context.Propiedades.AnyAsync(p => p.IdAgente == id);
            if (tienePropiedades)
                return BadRequest(new { mensaje = "No se puede eliminar un agente con propiedades asignadas" });

            _context.Usuarios.Remove(agente);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Agente eliminado exitosamente" });
        }

        // ============================================================
        // PROPIEDADES - LISTAR
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
        // PROPIEDADES - DETALLE (devuelve DTO, no modelo de BD)
        // ============================================================
        [HttpGet("propiedades/{id}")]
        public async Task<IActionResult> GetPropiedad(int id)
        {
            var p = await _context.Propiedades
                .Include(p => p.Agente)
                .Include(p => p.Imagenes)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (p == null) return NotFound();

            return Ok(new PropiedadResumenDTO
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
            });
        }

        // ============================================================
        // PROPIEDADES - EDITAR
        // ============================================================
        [HttpPut("propiedades/{id}")]
        public async Task<IActionResult> EditarPropiedad(int id, [FromBody] PropiedadUpdateDTO dto)
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
            propiedad.Estado = dto.Estado;

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Propiedad actualizada exitosamente" });
        }

        // ============================================================
        // PROPIEDADES - ELIMINAR
        // ============================================================
        [HttpDelete("propiedades/{id}")]
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

        // ============================================================
        // PROPIEDADES - CERRAR OPERACIÓN
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

        [HttpGet("clientes")]
        public async Task<IActionResult> GetClientes()
        {
            try
            {
                // 1. Consultar los clientes
                var clientes = await _context.Clientes.ToListAsync();

                // 2. Consultar los estados
                var estados = await _context.EstadosProspecto.ToListAsync();

                // 3. Combinar en memoria
                var resultado = clientes.Select(c =>
                {
                    var estado = estados.FirstOrDefault(e => e.Id == c.IdEstadoProspecto);
                    return new ClienteResumenDTO
                    {
                        Id = c.Id,
                        NombreCompleto = c.NombreCompleto ?? "",
                        Email = c.Email ?? "",
                        Telefono = c.Telefono ?? "",
                        FechaRegistro = c.FechaRegistro,
                        EstadoProspecto = estado?.Nombre ?? "Sin estado",
                        OrdenEstado = estado?.Orden ?? 0
                    };
                }).OrderBy(c => c.OrdenEstado).ToList();

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = ex.Message, detalle = ex.InnerException?.Message });
            }
        }

        // ============================================================
        // CITAS - LISTAR
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

            var resultado = citas.Select(c => new
            {
                c.Id,
                c.FechaCita,
                c.HoraInicio,
                c.HoraFin,
                c.FechaSolicitud,
                c.Observaciones,
                ClienteNombre = c.Cliente?.NombreCompleto ?? "",
                ClienteTelefono = c.Cliente?.Telefono ?? "",
                PropiedadTipo = c.Propiedad?.Tipo ?? "",
                PropiedadZona = c.Propiedad?.Zona ?? "",
                AgenteNombre = c.Agente?.NombreCompleto ?? "",
                EstadoNombre = c.EstadoCita?.NombreEstado ?? ""
            }).ToList();

            return Ok(resultado);
        }

        // ============================================================
        // COMISIONES - REPORTE
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
    }
}