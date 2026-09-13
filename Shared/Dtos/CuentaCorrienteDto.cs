namespace IurixBlazor.Shared.Dtos
{
    public class CuentaCorrienteDto
    {
        public int Id { get; set; }
        public int? PersonaId { get; set; }
        public DateTime? FechaApertura { get; set; }
        public decimal? SaldoApertura { get; set; }
        public bool Activa { get; set; } = true;

        public decimal? LimiteCredito { get; set; }
    }
}
