namespace IurixBlazor.Shared.Dtos
{
    public class CrearReciboDto
    {
        public int PersonaId { get; set; }
        public DateTime? Fecha { get; set; }
        public decimal Total { get; set; }
        public int MedioPagoId { get; set; }
        public int CajaMovimientoId { get; set; }
        public string? Observaciones { get; set; }
    }
}
