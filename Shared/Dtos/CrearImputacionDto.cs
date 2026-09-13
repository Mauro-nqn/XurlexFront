namespace IurixBlazor.Shared.Dtos
{
    public class CrearImputacionDto
    {
        public int MovimientoCreditoId { get; set; }
        public int MovimientoCargoId { get; set; }
        public decimal Importe { get; set; }
        public int? ReciboId { get; set; }
        public string OrigenTipo { get; set; } = "Recibo"; // Recibo|Reclasificacion|Ajuste
        public int? OrigenId { get; set; }
    }
}
