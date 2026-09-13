using IurixBlazor.Shared.Enums;

namespace IurixBlazor.Shared.Dtos
{
    public class AgendaDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public DateTime FechaAgendada { get; set; }
        public DateTime? FechaFin { get; set; }

        public int? TipoAgendamientoId { get; set; }
        public string TipoAgendamientoNombre { get; set; } = string.Empty;

        public EstadoAgenda EstadoAgenda { get; set; } = EstadoAgenda.Pendiente;

        public int? MovimientoId { get; set; }
        public string? MovimientoTitulo { get; set; }

        public int? PersonaId { get; set; }
        public string? PersonaNombre { get; set; }

        public int? UsuarioId { get; set; }
        public string? UsuarioNombre { get; set; }

        public string? Observaciones { get; set; }



        //  Notificaciones
        public bool NotificarApp { get; set; }
        public int? NotificarAppUsuarioId { get; set; }
        public string? NotificarAppUsuarioNombre { get; set; }

        public bool NotificarEmail { get; set; }
        public int? NotificarEmailUsuarioId { get; set; }
        public string? NotificarEmailUsuarioNombre { get; set; }
        public string? NotificarEmailTo { get; set; }
        public int? RecordatorioMinAntes { get; set; }




        //Google
        public string? GoogleEventId { get; set; }
        public string? GoogleCalendarId { get; set; }

        public bool GoogleRegistrado { get; set; }
        public string? GoogleHtmlLink { get; set; }
        public DateTimeOffset? GoogleLastSyncUtc { get; set; }

        public bool EsParaTodos { get; set; }
        public List<int> UsuarioIds { get; set; } = new();

        public int? GoogleUsuarioId { get; set; }           //  nuevo
        public string? GoogleEmailSnapshot { get; set; }    // opcional
    }
}
