namespace IurixBlazor.Shared.Dtos
{
    public class ManualFiscalesDto
    {
        public int? PuntoVentaNumero { get; set; }
        public int? Numero { get; set; }                // Nro de comprobante
        public DateTime? Fecha { get; set; }               // Fecha emisión
        public string? CAE { get; set; }                   // 14 dígitos
        public DateTime? CAEFchVto { get; set; }
    }
}
