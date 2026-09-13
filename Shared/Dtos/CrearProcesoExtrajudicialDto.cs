using System.Text.Json.Serialization;

namespace IurixBlazor.Shared.Dtos
{
    public class CrearProcesoExtrajudicialDto
    {
        [JsonIgnore] public int? Id { get; set; }  // solo cliente
        public string Tipo { get; set; } = string.Empty;
        public string Materia { get; set; } = string.Empty;
        public int? EstadoId { get; set; }
    }
}
