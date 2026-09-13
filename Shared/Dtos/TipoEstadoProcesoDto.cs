namespace IurixBlazor.Shared.Dtos
{
    public class TipoEstadoProcesoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string TipoProceso { get; set; } = string.Empty; // Judicial o Extrajudicial
    }
}
