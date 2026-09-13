namespace IurixBlazor.Shared.Dtos
{
    public class ReciboDto
    {
        public int Id { get; set; }
        public int? PersonaId { get; set; }
        public DateTime? Fecha { get; set; }
        public decimal Total { get; set; }
        public int? MedioPagoId { get; set; }
        public int? CajaMovimientoId { get; set; }
        public string? Observaciones { get; set; }


        public bool Anulado { get; set; }
        public DateTime? FechaAnulacion { get; set; }
        public int? UsuarioAnuloId { get; set; }
        public string? MotivoAnulacion { get; set; }


    }
}
