namespace IurixBlazor.Shared.Dtos
{

    public class GestionDto
    {
        public int Id { get; set; }
        public int ResponsableId { get; set; }
        public string ResponsableNombre { get; set; } = string.Empty;
        public string TipoGestion { get; set; } = string.Empty;
        public int? PersonaId { get; set; }
        public string? PersonaNombre { get; set; } // Nombre completo o Razón Social
        public int? PresupuestoId { get; set; }
        public string? PresupuestoNombre { get; set; }
        public int? GrupoId { get; set; }
        public string GrupoNombre { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFinalizacion { get; set; }

        public ProcesoJudicialDto? ProcesoJudicial { get; set; }
        public ProcesoExtrajudicialDto? ProcesoExtrajudicial { get; set; }
    }

}
