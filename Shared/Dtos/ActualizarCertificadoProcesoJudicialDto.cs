namespace IurixBlazor.Shared.Dtos
{
    public class ActualizarCertificadoProcesoJudicialDto
    {
        public string? NumeroCertificadoDeuda { get; set; }
        public decimal? MontoDemanda { get; set; }
        public DateTime? FechaEmisionCertificado { get; set; }
        public DateTime? FechaLiquidacion { get; set; }
    }

}
