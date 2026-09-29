using System.Globalization;
using System.Security.Claims;
using System.Text;
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
    [Authorize(Roles = "Cliente")]
    public class ClienteApiController : ControllerBase
    {
        private readonly InmobiliariaContext _context;

        private static readonly string[] DiasSemana =
            { "Domingo", "Lunes", "Martes", "Miercoles", "Jueves", "Viernes", "Sabado" };

        public ClienteApiController(InmobiliariaContext context)
        {
            _context = context;
        }

        // ============================================================
        // GET: /api/clienteapi/citas (citas del cliente logueado)
        // ============================================================
        [HttpGet("citas")]
        public async Task<IActionResult> MisCitas()
        {
            var cliente = await ObtenerCliente();
            if (cliente == null) return Ok(new List<object>());

            var citas = await _context.Citas
                .Include(c => c.Cliente)
                .Include(c => c.Propiedad)
                .Include(c => c.Agente)
                .Include(c => c.EstadoCita)
                .Where(c => c.IdCliente == cliente.Id)
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
                PropiedadTipo = c.Propiedad?.Tipo ?? "",
                PropiedadZona = c.Propiedad?.Zona ?? "",
                PropiedadDireccion = c.Propiedad?.Direccion ?? "",
                AgenteNombre = c.Agente?.NombreCompleto ?? "",
                EstadoNombre = c.EstadoCita?.NombreEstado ?? ""
            }).ToList();

            return Ok(resultado);
        }

        // ============================================================
        // POST: /api/clienteapi/citas (reservar visita)
        // ============================================================
        [HttpPost("citas")]
        public async Task<IActionResult> CrearCita([FromBody] CitaCreateDTO dto)
        {
            var cliente = await ObtenerCliente();
            if (cliente == null)
                return BadRequest(new { mensaje = "El usuario no tiene perfil de cliente" });

            if (dto.IdPropiedad <= 0)
                return BadRequest(new { mensaje = "Propiedad no válida" });

            var propiedad = await _context.Propiedades.FindAsync(dto.IdPropiedad);
            if (propiedad == null)
                return NotFound(new { mensaje = "La propiedad no existe" });

            if (propiedad.Estado != "Disponible")
                return BadRequest(new { mensaje = "La propiedad ya no está disponible" });

            if (dto.Fecha.Date < DateTime.Today)
                return BadRequest(new { mensaje = "La fecha debe ser de hoy en adelante" });

            if (dto.HoraInicio <= TimeSpan.Zero)
                return BadRequest(new { mensaje = "Indica una hora válida para la visita" });

            if (dto.HoraFin <= TimeSpan.Zero)
                dto.HoraFin = dto.HoraInicio.Add(TimeSpan.FromHours(1));

            if (dto.HoraFin <= dto.HoraInicio)
                return BadRequest(new { mensaje = "La hora de fin debe ser mayor a la de inicio" });

            // Buscar disponibilidad del agente para ese día
            var diaRequest = DiasSemana[(int)dto.Fecha.DayOfWeek];
            var diaNormalizado = NormalizarDia(diaRequest);

            var disponibilidadAgente = await _context.DisponibilidadesAgente
                .Where(d => d.IdAgente == propiedad.IdAgente && d.Activo)
                .ToListAsync();

            var disponibilidad = disponibilidadAgente
                .FirstOrDefault(d => NormalizarDia(d.DiaSemana ?? "") == diaNormalizado)
                ?? disponibilidadAgente.FirstOrDefault();

            if (disponibilidad == null)
                return BadRequest(new { mensaje = "El agente de la propiedad no tiene disponibilidad definida" });

            var cita = new Cita
            {
                FechaCita = dto.Fecha,
                HoraInicio = dto.HoraInicio,
                HoraFin = dto.HoraFin,
                FechaSolicitud = DateTime.UtcNow,
                Observaciones = dto.Observaciones,
                IdCliente = cliente.Id,
                IdPropiedad = propiedad.Id,
                IdAgente = propiedad.IdAgente,
                IdDisponibilidad = disponibilidad.Id,
                IdEstadoCita = 1
            };

            _context.Citas.Add(cita);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Visita reservada exitosamente", id = cita.Id });
        }

        // ============================================================
        // PUT: /api/clienteapi/citas/{id}/cancelar
        // ============================================================
        [HttpPut("citas/{id}/cancelar")]
        public async Task<IActionResult> CancelarCita(int id)
        {
            var cliente = await ObtenerCliente();
            if (cliente == null)
                return BadRequest(new { mensaje = "El usuario no tiene perfil de cliente" });

            var cita = await _context.Citas
                .FirstOrDefaultAsync(c => c.Id == id && c.IdCliente == cliente.Id);

            if (cita == null)
                return NotFound(new { mensaje = "La cita no existe" });

            if (cita.IdEstadoCita != 1 && cita.IdEstadoCita != 2)
                return BadRequest(new { mensaje = "Solo puedes cancelar citas pendientes o confirmadas" });

            cita.IdEstadoCita = 3;
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Cita cancelada exitosamente" });
        }

        // ============================================================
        // MÉTODOS AUXILIARES
        // ============================================================
        private async Task<Cliente?> ObtenerCliente()
        {
            var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claimId) || !int.TryParse(claimId, out int usuarioId))
                return null;

            return await _context.Clientes
                .FirstOrDefaultAsync(c => c.IdUsuario == usuarioId);
        }

        private static string NormalizarDia(string dia)
        {
            var sinAcentos = dia.Normalize(NormalizationForm.FormD)
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                .ToArray();
            return new string(sinAcentos).ToLowerInvariant();
        }
    }
}