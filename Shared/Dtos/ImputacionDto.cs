namespace IurixBlazor.Shared.Dtos
{
    public class ImputacionDto
    {
        public int Id { get; set; }
        public int? MovimientoCreditoId { get; set; }
        public int? MovimientoCargoId { get; set; }
        public decimal? Importe { get; set; }
        public int? ReciboId { get; set; }
        public string? OrigenTipo { get; set; }
        public int? OrigenId { get; set; }
    }
}
