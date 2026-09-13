namespace IurixBlazor.Shared.Dtos
{
    public class CrearAgendaDesdeMovimientoDto
    {
        public int MovimientoId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public DateTime FechaInicioLocal { get; set; }           // Unspecified (hora AR)

        public DateTime? FechaFin { get; set; }
        public int DuracionMin { get; set; }                     // construye FechaFin = inicio + duración
        public int? TipoAgendamientoId { get; set; }             // null = “Seleccione”
        public string? Observaciones { get; set; }

        // Notificaciones
        public bool NotificarApp { get; set; }
        public int? NotificarAppUsuarioId { get; set; }
        public bool NotificarEmail { get; set; }
        public int? NotificarEmailUsuarioId { get; set; }
        public string? NotificarEmailTo { get; set; }
        public int? RecordatorioMinAntes { get; set; }

        // (opcional) asociar Persona/Usuario asignado
        public int? PersonaId { get; set; }
        public int? UsuarioId { get; set; }

        public bool EsParaTodos { get; set; }
        public List<int>? UsuarioIds { get; set; } = new();

        public int? GoogleUsuarioId { get; set; }
        public string? GoogleEmailSnapshot { get; set; }   // opcional
    }

}
