namespace IurixBlazor.Shared.Dtos
{
    public class FacturaImpresionDto : FacturaDto
    {
        public string NombreTitularEmisor { get; set; } = "";
        public string CondicionFiscalEmisor { get; set; } = "";
        public string CuitEmisor { get; set; } = "";
        public string IIBBEmisor { get; set; } = "";
        public string InicioActividadesEmisor { get; set; } = "";
        public string DomicilioEmisor { get; set; } = "";
        public string ProvinciaEmisor { get; set; } = "";
        public string EmailEmisor { get; set; } = "";
        public string Logo { get; set; } = "";
        public string QRUrl { get; set; } = "";

        
    }
}
