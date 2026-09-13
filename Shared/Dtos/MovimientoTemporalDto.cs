namespace IurixBlazor.Shared.Dtos
{
    public class MovimientoTemporalDto
    {
        public string? Titulo { get; set; }
        public string? Detalle { get; set; }
        public DateTime? Fecha { get; set; }

        public List<ArchivoTemporalDto> Archivos { get; set; } = new();
    }
}
