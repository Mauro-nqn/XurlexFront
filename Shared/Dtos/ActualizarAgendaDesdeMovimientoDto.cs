namespace IurixBlazor.Shared.Dtos
{
    public class ActualizarAgendaDesdeMovimientoDto
    {
        public string Titulo { get; set; } = string.Empty;

        // Local AR (Unspecified); el backend lo pasa a UTC
        public DateTime FechaInicioLocal { get; set; }
        public DateTime? FechaFin { get; set; }
        public int DuracionMin { get; set; }

        public int? TipoAgendamientoId { get; set; }
        public string? Observaciones { get; set; }

        // Notificaciones
        public bool NotificarApp { get; set; }
        public int? NotificarAppUsuarioId { get; set; }
        public bool NotificarEmail { get; set; }
        public int? NotificarEmailUsuarioId { get; set; }
        public string? NotificarEmailTo { get; set; }
        public int? RecordatorioMinAntes { get; set; }

        public int? PersonaId { get; set; }
        public int? UsuarioId { get; set; }

        public bool EsParaTodos { get; set; }
        public List<int>? UsuarioIds { get; set; } = new();

        public int? GoogleUsuarioId { get; set; }
        public string? GoogleEmailSnapshot { get; set; }   // opcional
    }
}
