namespace IurixBlazor.Shared.Dtos
{
    public class ActualizarGestionDto
    {
        public string TipoGestion { get; set; } = string.Empty;
        public int? GrupoId { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFinalizacion { get; set; }
    }

}
