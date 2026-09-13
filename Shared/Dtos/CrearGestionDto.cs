using System.Text.Json.Serialization;

namespace IurixBlazor.Shared.Dtos
{
    public class CrearGestionDto
    {
        [JsonIgnore] public int? Id { get; set; }  // solo cliente
        public string TipoGestion { get; set; } = string.Empty; // Judicial o Extrajudicial
        public int ResponsableId { get; set; }
        public int? PersonaId { get; set; }   // Opcional
        public int? PresupuestoId { get; set; }// Opcional
        public int? GrupoId { get; set; }// Opcional
        public DateTime FechaInicio { get; set; }

        public CrearProcesoJudicialDto? ProcesoJudicial { get; set; }
        public CrearProcesoExtrajudicialDto? ProcesoExtrajudicial { get; set; }
    }
}
