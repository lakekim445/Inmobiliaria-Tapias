using System.Security.Claims;
using InmobiliariaAPI.Data;
using InmobiliariaAPI.DTOs;
using InmobiliariaAPI.Models;
using InmobiliariaAPI.Services;
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

            var errorHorario = AgendaReglas.ValidarHorario(dto.HoraInicio, dto.HoraFin);
            if (errorHorario != null)
                return BadRequest(new { mensaje = errorHorario });

            // Regla: cada visita bloquea 2 horas del agente.
            var inicioDia = DateTime.SpecifyKind(dto.Fecha.Date, DateTimeKind.Utc);
            var finDia = inicioDia.AddDays(1);

            var citasDelDia = await _context.Citas
                .Where(c => c.IdAgente == propiedad.IdAgente && c.FechaCita >= inicioDia && c.FechaCita < finDia)
                .ToListAsync();

            var horaAjustada = dto.HoraInicio;
            var ajustada = AgendaReglas.HayConflicto(dto.HoraInicio, citasDelDia);

            // Si el horario pedido no está libre, se asigna automáticamente el siguiente
            // horario libre del agente ese mismo día (dentro de 9:00-18:00).
            if (ajustada)
            {
                var libre = AgendaReglas.ProximaDisponible(dto.HoraInicio, citasDelDia);
                if (!libre.HasValue)
                    return BadRequest(new { mensaje = "El agente no tiene horarios libres ese día dentro de 9:00-18:00 (cada visita bloquea 2 horas)." });

                horaAjustada = libre.Value;
                dto.HoraFin = horaAjustada.Add(TimeSpan.FromHours(1));
                dto.HoraInicio = horaAjustada;
            }

            // Disponibilidad del agente (opcional): si tiene horarios configurados en la
            // tabla, se usa el que coincida con el día; si no, la visita se agenda igual.
            var disponibilidadAgente = await _context.DisponibilidadesAgente
                .Where(d => d.IdAgente == propiedad.IdAgente && d.Activo)
                .ToListAsync();

            var disponibilidad = AgendaReglas.BuscarDisponibilidad(dto.Fecha, disponibilidadAgente);

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
                IdDisponibilidad = disponibilidad?.Id,
                IdEstadoCita = 1
            };

            _context.Citas.Add(cita);
            await _context.SaveChangesAsync();

            // Avisar al agente de la nueva solicitud de visita
            CitaFlujo.Notificar(_context, propiedad.IdAgente,
                $"{cliente.NombreCompleto} solicitó una visita para {propiedad.Tipo} en {propiedad.Zona} el {dto.Fecha:dd/MM/yyyy} a las {dto.HoraInicio:hh\\:mm}.",
                "Confirmacion_Cita", cita.Id, propiedad.Id);
            await _context.SaveChangesAsync();

            var mensaje = ajustada
                ? $"¡Visita reservada a las {dto.HoraInicio:hh\\:mm}! (el horario que elegiste estaba ocupado y se asignó el siguiente libre)."
                : "¡Visita reservada exitosamente!";

            return Ok(new { mensaje, id = cita.Id, horaInicio = dto.HoraInicio.ToString(@"hh\:mm"), ajustada });
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

            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            CitaFlujo.RegistrarHistorial(_context, cita, CitaFlujo.Cancelada, usuarioId, "Cancelada por el cliente");

            // Avisar al agente de la cancelación
            CitaFlujo.Notificar(_context, cita.IdAgente,
                $"El cliente {cliente.NombreCompleto} CANCELÓ su visita del {cita.FechaCita:dd/MM/yyyy} a las {cita.HoraInicio:hh\\:mm}.",
                "Cancelacion_Cita", cita.Id, cita.IdPropiedad);

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Cita cancelada exitosamente" });
        }

        // ============================================================
        // NOTIFICACIONES DEL CLIENTE
        // ============================================================
        [HttpGet("notificaciones")]
        public async Task<IActionResult> Notificaciones()
        {
            var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claimId) || !int.TryParse(claimId, out int usuarioId))
                return Ok(new List<object>());

            var lista = await _context.Notificaciones
                .Where(n => n.IdUsuario == usuarioId)
                .OrderByDescending(n => n.FechaEnvio)
                .ToListAsync();

            return Ok(lista.Select(n => new
            {
                n.Id,
                n.Tipo,
                n.Mensaje,
                n.Leida,
                n.FechaEnvio,
                n.IdCita,
                n.IdPropiedad
            }).ToList());
        }

        [HttpGet("notificaciones/noleidas")]
        public async Task<IActionResult> NoLeidas()
        {
            var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claimId) || !int.TryParse(claimId, out int usuarioId))
                return Ok(0);

            var contador = await _context.Notificaciones
                .CountAsync(n => n.IdUsuario == usuarioId && !n.Leida);
            return Ok(contador);
        }

        [HttpPut("notificaciones/{id}/leer")]
        public async Task<IActionResult> MarcarLeida(int id)
        {
            var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claimId) || !int.TryParse(claimId, out int usuarioId))
                return Unauthorized();

            var notif = await _context.Notificaciones
                .FirstOrDefaultAsync(n => n.Id == id && n.IdUsuario == usuarioId);

            if (notif == null)
                return NotFound();

            notif.Leida = true;
            notif.FechaLectura = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Notificación marcada como leída" });
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
    }
}
