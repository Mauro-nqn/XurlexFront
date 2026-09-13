namespace IurixBlazor.Shared.Dtos
{
    public class ImputacionDetalleDto
    {
        public DateTime Fecha { get; set; }
        public string Concepto { get; set; } = "";
        public decimal Importe { get; set; }
        public int? PresupuestoId { get; set; }
        public int? FacturaId { get; set; }
    }
}
