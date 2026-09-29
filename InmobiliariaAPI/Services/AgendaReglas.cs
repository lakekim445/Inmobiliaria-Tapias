using System.Globalization;
using System.Text;
using InmobiliariaAPI.Models;

namespace InmobiliariaAPI.Services
{
    public static class AgendaReglas
    {
        // Cada visita bloquea 2 horas del agente:
        // ida al lugar, mostrar el inmueble, explicar y volver.
        public static readonly TimeSpan BloquesPorVisita = TimeSpan.FromHours(2);

        // Horario permitido para agendar visitas: 9:00 a 18:00.
        public static readonly TimeSpan HoraMinima = new TimeSpan(9, 0, 0);
        public static readonly TimeSpan HoraMaxima = new TimeSpan(18, 0, 0);

        /// <summary>Valida que la visita esté dentro del horario 9:00-18:00. Devuelve null si es válida.</summary>
        public static string? ValidarHorario(TimeSpan horaInicio, TimeSpan horaFin)
        {
            if (horaInicio < HoraMinima || horaInicio >= HoraMaxima)
                return "Solo puedes agendar visitas entre las 9:00 y las 18:00.";

            if (horaFin <= horaInicio)
                return "La hora de fin debe ser mayor a la de inicio.";

            if (horaFin > HoraMaxima)
                return "La visita debe terminar antes de las 18:00.";

            return null;
        }

        private static readonly string[] DiasSemana =
            { "Domingo", "Lunes", "Martes", "Miercoles", "Jueves", "Viernes", "Sabado" };

        // Una nueva cita NO debe solaparse con el bloque de 2h de otra cita del mismo agente.
        public static bool HayConflicto(TimeSpan horaInicioNueva, IEnumerable<Cita> citasDelDia)
        {
            var iniNueva = horaInicioNueva;
            var finNueva = horaInicioNueva.Add(BloquesPorVisita);

            foreach (var c in citasDelDia)
            {
                if (c.IdEstadoCita == 3) continue; // cancelada no bloquea
                if (c.IdEstadoCita == 4) continue; // completada no bloquea

                var iniExistente = c.HoraInicio;
                var finExistente = c.HoraInicio.Add(BloquesPorVisita);

                if (iniNueva < finExistente && iniExistente < finNueva)
                    return true;
            }

            return false;
        }

        // Devuelve la siguiente hora libre para el agente en ese día, comenzando en horaInicio
        // y avanzando en pasos de 30 min, PERO SOLO dentro del horario 9:00-18:00
        // (la visita dura 1 hora, así que la última hora de inicio posible es las 17:00).
        // Devuelve null si no queda ningún horario libre ese día.
        public static TimeSpan? ProximaDisponible(TimeSpan horaInicio, IEnumerable<Cita> citasDelDia)
        {
            if (!HayConflicto(horaInicio, citasDelDia))
                return horaInicio;

            var candidata = horaInicio.Add(TimeSpan.FromMinutes(30));
            var ultimaInicio = new TimeSpan(17, 0, 0);

            while (candidata <= ultimaInicio)
            {
                if (!HayConflicto(candidata, citasDelDia))
                    return candidata;
                candidata = candidata.Add(TimeSpan.FromMinutes(30));
            }

            return null;
        }

        public static string DiaSemanaEspanol(DayOfWeek dia) => DiasSemana[(int)dia];

        public static string NormalizarTexto(string texto)
        {
            var sinAcentos = texto.Normalize(NormalizationForm.FormD)
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                .ToArray();
            return new string(sinAcentos).ToLowerInvariant();
        }

        public static DisponibilidadAgente? BuscarDisponibilidad(
            DateTime fecha,
            IEnumerable<DisponibilidadAgente> disponibilidades)
        {
            var dia = DiaSemanaEspanol(fecha.DayOfWeek);
            var diaNormalizado = NormalizarTexto(dia);

            return disponibilidades
                .FirstOrDefault(d => NormalizarTexto(d.DiaSemana ?? "") == diaNormalizado)
                ?? disponibilidades.FirstOrDefault();
        }
    }
}