namespace IurixBlazor.Shared.Dtos
{
    public class FacturaResumenDto
    {
        public int FacturaId { get; set; }
        public string? TipoComprobante { get; set; }
        public string? PuntoVenta { get; set; }
        public string? Numero { get; set; }        // p.ej. "A 0001-00001234"
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public string? Estado { get; set; }        // opcional
        public string? CbteTipo { get; set; }      // opcional: "FA", "NC", etc.
    }
}
