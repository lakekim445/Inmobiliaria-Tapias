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
    [Authorize(Roles = "Agente")]
    public class AgenteApiController : ControllerBase
    {
        private readonly InmobiliariaContext _context;

        public AgenteApiController(InmobiliariaContext context)
        {
            _context = context;
        }

        // ============================================================
        // GET: /api/agenteapi/citas (citas asignadas al agente logueado)
        // ============================================================
        [HttpGet("citas")]
        public async Task<IActionResult> MisCitas()
        {
            var agenteId = ObtenerAgenteId();
            if (agenteId == 0)
                return Ok(new List<object>());

            var citas = await _context.Citas
                .Include(c => c.Cliente)
                .Include(c => c.Propiedad)
                .Include(c => c.EstadoCita)
                .Where(c => c.IdAgente == agenteId)
                .OrderByDescending(c => c.FechaCita)
                .ThenByDescending(c => c.HoraInicio)
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
                ClienteEmail = c.Cliente?.Email ?? "",
                PropiedadTipo = c.Propiedad?.Tipo ?? "",
                PropiedadZona = c.Propiedad?.Zona ?? "",
                PropiedadDireccion = c.Propiedad?.Direccion ?? "",
                EstadoNombre = c.EstadoCita?.NombreEstado ?? ""
            }).ToList();

            return Ok(resultado);
        }

        // ============================================================
        // GET: /api/agenteapi/clientes (lista para registrar citas)
        // ============================================================
        [HttpGet("clientes")]
        public async Task<IActionResult> GetClientes()
        {
            var clientes = await _context.Clientes
                .OrderBy(c => c.NombreCompleto)
                .ToListAsync();

            var estados = await _context.EstadosProspecto.ToListAsync();

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
            }).ToList();

            return Ok(resultado);
        }

        // ============================================================
        // POST: /api/agenteapi/citas (registrar visita manualmente)
        // ============================================================
        [HttpPost("citas")]
        public async Task<IActionResult> CrearCita([FromBody] CitaManualCreateDTO dto)
        {
            var agenteId = ObtenerAgenteId();
            if (agenteId == 0)
                return Unauthorized();

            if (dto.IdPropiedad <= 0)
                return BadRequest(new { mensaje = "Selecciona una propiedad" });

            var propiedad = await _context.Propiedades.FindAsync(dto.IdPropiedad);
            if (propiedad == null)
                return BadRequest(new { mensaje = "La propiedad no existe" });

            if (propiedad.IdAgente != agenteId)
                return BadRequest(new { mensaje = "Solo puedes agendar visitas en tus propias propiedades" });

            // Definir el cliente de la cita:
            // - IdCliente > 0  -> cliente registrado en el sistema
            // - IdCliente == 0 -> cliente sin cuenta (ej. contacto por WhatsApp)
            Cliente? cliente;
            if (dto.IdCliente > 0)
            {
                cliente = await _context.Clientes.FindAsync(dto.IdCliente);
                if (cliente == null)
                    return BadRequest(new { mensaje = "El cliente no existe" });
            }
            else
            {
                if (string.IsNullOrWhiteSpace(dto.NombreClienteNuevo))
                    return BadRequest(new { mensaje = "Ingresa el nombre del cliente" });

                cliente = new Cliente
                {
                    NombreCompleto = dto.NombreClienteNuevo.Trim(),
                    Telefono = string.IsNullOrWhiteSpace(dto.TelefonoClienteNuevo)
                        ? "00000000"
                        : dto.TelefonoClienteNuevo.Trim(),
                    Email = dto.EmailClienteNuevo ?? "",
                    FechaRegistro = DateTime.UtcNow,
                    IdUsuario = null,
                    IdEstadoProspecto = 1
                };

                _context.Clientes.Add(cliente);
                await _context.SaveChangesAsync();
            }

            if (dto.Fecha.Date < DateTime.Today)
                return BadRequest(new { mensaje = "La fecha debe ser de hoy en adelante" });

            if (dto.HoraInicio <= TimeSpan.Zero)
                return BadRequest(new { mensaje = "Indica una hora válida para la visita" });

            if (dto.HoraFin <= TimeSpan.Zero)
                dto.HoraFin = dto.HoraInicio.Add(TimeSpan.FromHours(1));

            var errorHorario = AgendaReglas.ValidarHorario(dto.HoraInicio, dto.HoraFin);
            if (errorHorario != null)
                return BadRequest(new { mensaje = errorHorario });

            // Regla: cada visita bloquea 2 horas del agente.
            var inicioDia = DateTime.SpecifyKind(dto.Fecha.Date, DateTimeKind.Utc);
            var finDia = inicioDia.AddDays(1);

            var citasDelDia = await _context.Citas
                .Where(c => c.IdAgente == agenteId && c.FechaCita >= inicioDia && c.FechaCita < finDia)
                .ToListAsync();

            var horaAjustada = dto.HoraInicio;
            var ajustada = AgendaReglas.HayConflicto(dto.HoraInicio, citasDelDia);

            // Si el horario pedido no está libre, se asigna automáticamente el siguiente
            // horario libre del agente ese mismo día (dentro de 9:00-18:00).
            if (ajustada)
            {
                var libre = AgendaReglas.ProximaDisponible(dto.HoraInicio, citasDelDia);
                if (!libre.HasValue)
                    return BadRequest(new { mensaje = "No tienes horarios libres ese día dentro de 9:00-18:00 (cada visita bloquea 2 horas)." });

                horaAjustada = libre.Value;
                dto.HoraFin = horaAjustada.Add(TimeSpan.FromHours(1));
                dto.HoraInicio = horaAjustada;
            }

            // Disponibilidad (opcional): si existe fila, se asocia; si no, se agenda igual.
            var disponibilidades = await _context.DisponibilidadesAgente
                .Where(d => d.IdAgente == agenteId && d.Activo)
                .ToListAsync();

            var disponibilidad = AgendaReglas.BuscarDisponibilidad(dto.Fecha, disponibilidades);

            var cita = new Cita
            {
                FechaCita = dto.Fecha,
                HoraInicio = dto.HoraInicio,
                HoraFin = dto.HoraFin,
                FechaSolicitud = DateTime.UtcNow,
                Observaciones = dto.Observaciones,
                IdCliente = cliente.Id,
                IdPropiedad = propiedad.Id,
                IdAgente = agenteId,
                IdDisponibilidad = disponibilidad?.Id,
                IdEstadoCita = 1
            };

            _context.Citas.Add(cita);
            await _context.SaveChangesAsync();

            // Avisar al cliente registrado que el agente agendó una visita
            if (cliente.IdUsuario != null)
            {
                CitaFlujo.Notificar(_context, cliente.IdUsuario.Value,
                    $"Se agendó una visita para el {dto.Fecha:dd/MM/yyyy} a las {dto.HoraInicio:hh\\:mm} en {propiedad.Tipo} ({propiedad.Zona}).",
                    "Confirmacion_Cita", cita.Id, propiedad.Id);
                await _context.SaveChangesAsync();
            }

            var mensaje = ajustada
                ? $"Cita registrada a las {dto.HoraInicio:hh\\:mm} (el horario pedido estaba ocupado y se asignó el siguiente libre)."
                : "Cita registrada exitosamente";

            return Ok(new { mensaje, id = cita.Id, horaInicio = dto.HoraInicio.ToString(@"hh\:mm"), ajustada });
        }

        // ============================================================
        // PUT: /api/agenteapi/citas/{id}/confirmar
        // ============================================================
        [HttpPut("citas/{id}/confirmar")]
        public async Task<IActionResult> ConfirmarCita(int id)
        {
            var agenteId = ObtenerAgenteId();

            var cita = await _context.Citas
                .FirstOrDefaultAsync(c => c.Id == id && c.IdAgente == agenteId);

            if (cita == null)
                return NotFound(new { mensaje = "La cita no existe" });

            if (cita.IdEstadoCita != CitaFlujo.Pendiente)
                return BadRequest(new { mensaje = "Solo puedes confirmar citas pendientes" });

            CitaFlujo.RegistrarHistorial(_context, cita, CitaFlujo.Confirmada, agenteId, "Confirmada por el agente");

            var cliente = await _context.Clientes.FindAsync(cita.IdCliente);
            if (cliente?.IdUsuario != null)
            {
                CitaFlujo.Notificar(_context, cliente.IdUsuario.Value,
                    $"Tu visita del {cita.FechaCita:dd/MM/yyyy} a las {cita.HoraInicio:hh\\:mm} fue CONFIRMADA.",
                    "Confirmacion_Cita", cita.Id, cita.IdPropiedad);
            }

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Cita confirmada" });
        }

        // ============================================================
        // PUT: /api/agenteapi/citas/{id}/completar
        // ============================================================
        [HttpPut("citas/{id}/completar")]
        public async Task<IActionResult> CompletarCita(int id)
        {
            var agenteId = ObtenerAgenteId();

            var cita = await _context.Citas
                .FirstOrDefaultAsync(c => c.Id == id && c.IdAgente == agenteId);

            if (cita == null)
                return NotFound(new { mensaje = "La cita no existe" });

            if (cita.IdEstadoCita != CitaFlujo.Confirmada && cita.IdEstadoCita != CitaFlujo.Pendiente)
                return BadRequest(new { mensaje = "Solo puedes completar citas confirmadas o pendientes" });

            CitaFlujo.RegistrarHistorial(_context, cita, CitaFlujo.Completada, agenteId, "Visita realizada");

            var cliente = await _context.Clientes.FindAsync(cita.IdCliente);
            if (cliente?.IdUsuario != null)
            {
                CitaFlujo.Notificar(_context, cliente.IdUsuario.Value,
                    $"Tu visita del {cita.FechaCita:dd/MM/yyyy} fue registrada como COMPLETADA.",
                    "Recordatorio_Cita", cita.Id, cita.IdPropiedad);
            }

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Visita completada" });
        }

        // ============================================================
        // PUT: /api/agenteapi/citas/{id}/cancelar
        // ============================================================
        [HttpPut("citas/{id}/cancelar")]
        public async Task<IActionResult> CancelarCita(int id)
        {
            var agenteId = ObtenerAgenteId();

            var cita = await _context.Citas
                .FirstOrDefaultAsync(c => c.Id == id && c.IdAgente == agenteId);

            if (cita == null)
                return NotFound(new { mensaje = "La cita no existe" });

            if (cita.IdEstadoCita != 1 && cita.IdEstadoCita != 2)
                return BadRequest(new { mensaje = "Solo puedes cancelar citas pendientes o confirmadas" });

            CitaFlujo.RegistrarHistorial(_context, cita, CitaFlujo.Cancelada, agenteId, "Cancelada por el agente");

            var cliente = await _context.Clientes.FindAsync(cita.IdCliente);
            if (cliente?.IdUsuario != null)
            {
                CitaFlujo.Notificar(_context, cliente.IdUsuario.Value,
                    $"Tu visita del {cita.FechaCita:dd/MM/yyyy} a las {cita.HoraInicio:hh\\:mm} fue CANCELADA.",
                    "Cancelacion_Cita", cita.Id, cita.IdPropiedad);
            }

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Cita cancelada exitosamente" });
        }

        // ============================================================
        // NOTIFICACIONES DEL AGENTE
        // ============================================================
        [HttpGet("notificaciones")]
        public async Task<IActionResult> Notificaciones()
        {
            var agenteId = ObtenerAgenteId();

            var lista = await _context.Notificaciones
                .Where(n => n.IdUsuario == agenteId)
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
            var agenteId = ObtenerAgenteId();
            var contador = await _context.Notificaciones
                .CountAsync(n => n.IdUsuario == agenteId && !n.Leida);
            return Ok(contador);
        }

        [HttpPut("notificaciones/{id}/leer")]
        public async Task<IActionResult> MarcarLeida(int id)
        {
            var agenteId = ObtenerAgenteId();
            var notif = await _context.Notificaciones
                .FirstOrDefaultAsync(n => n.Id == id && n.IdUsuario == agenteId);

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
        private int ObtenerAgenteId()
        {
            // El agente se identifica con el Id de su Usuario (rol Agente).
            var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claimId) || !int.TryParse(claimId, out int usuarioId))
                return 0;

            return usuarioId;
        }
    }
}