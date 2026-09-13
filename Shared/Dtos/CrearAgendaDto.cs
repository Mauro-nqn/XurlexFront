using IurixBlazor.Shared.Enums;

namespace IurixBlazor.Shared.Dtos
{
    public class CrearAgendaDto
    {
        public string Titulo { get; set; } = string.Empty;
        public DateTime FechaAgendada { get; set; }
        public DateTime? FechaFin { get; set; }        

        public int? MovimientoId { get; set; }
        public int? PersonaId { get; set; }

        public int? UsuarioId { get; set; }
        public string? PersonaNombre { get; set; }
        public int? TipoAgendamientoId { get; set; }
        public EstadoAgenda EstadoAgenda { get; set; } = EstadoAgenda.Pendiente;

        public string? Observaciones { get; set; }
        public bool? GoogleRegistrado { get; set; }
        public string? GoogleHtmlLink { get; set; }
        public DateTimeOffset? GoogleLastSyncUtc { get; set; }

        public int? GoogleUsuarioId { get; set; }           //  nuevo
        public string? GoogleEmailSnapshot { get; set; }    // opcional


        // NUEVOS:
        public bool EsParaTodos { get; set; } = true;
        public List<int>? UsuarioIds { get; set; } // visibilidad específica


    }
}
