namespace IurixBlazor.Shared.Dtos
{
    public class ProcesoExtrajudicialDto
    {
        public int Id { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Materia { get; set; } = string.Empty;
        public int? EstadoId { get; set; }
        public string? EstadoNombre { get; set; } = string.Empty;
    }
}
