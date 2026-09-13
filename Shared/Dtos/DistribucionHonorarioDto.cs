namespace IurixBlazor.Shared.Dtos
{
    public class DistribucionHonorarioDto
    {
        public int Id { get; set; }
        public string? Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }

        public ICollection<DistribucionDetalleDto> Detalles { get; set; } = new List<DistribucionDetalleDto>();
    }
}
