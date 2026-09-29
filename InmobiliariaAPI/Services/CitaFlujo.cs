using InmobiliariaAPI.Data;
using InmobiliariaAPI.Models;

namespace InmobiliariaAPI.Services
{
    /// <summary>
    /// Flujo de las citas: cambia de estado, registra historial
    /// y genera notificaciones entre cliente y agente.
    /// </summary>
    public static class CitaFlujo
    {
        public const int Pendiente = 1;
        public const int Confirmada = 2;
        public const int Cancelada = 3;
        public const int Completada = 4;

        /// <summary>Cambia el estado de la cita y guarda el cambio en historial_estado_cita.</summary>
        public static void RegistrarHistorial(
            InmobiliariaContext ctx,
            Cita cita,
            int estadoNuevo,
            int idUsuario,
            string? motivo = null)
        {
            ctx.HistorialEstadosCita.Add(new HistorialEstadoCita
            {
                EstadoAnterior = cita.IdEstadoCita,
                EstadoNuevo = estadoNuevo,
                Motivo = motivo,
                FechaCambio = DateTime.UtcNow,
                IdCita = cita.Id,
                IdUsuario = idUsuario
            });

            cita.IdEstadoCita = estadoNuevo;
        }

        /// <summary>Crea una notificación para un usuario (no guarda cambios).</summary>
        public static void Notificar(
            InmobiliariaContext ctx,
            int idUsuario,
            string mensaje,
            string tipo,
            int? idCita = null,
            int? idPropiedad = null)
        {
            ctx.Notificaciones.Add(new Notificacion
            {
                Tipo = tipo,
                Mensaje = mensaje,
                Leida = false,
                FechaEnvio = DateTime.UtcNow,
                IdUsuario = idUsuario,
                IdCita = idCita,
                IdPropiedad = idPropiedad
            });
        }
    }
}