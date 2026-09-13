namespace IurixBlazor.Shared.Dtos
{
    public class CertificadoApremioParseResultDto
    {
        public string? Numero { get; set; }
        public decimal? Monto { get; set; }
        public string? MontoEnLetras { get; set; }
        public DateTime? FechaEmision { get; set; }
        public DateTime? FechaLiquidacion { get; set; }
    }

}
