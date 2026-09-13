namespace IurixBlazor.Shared.Dtos
{
    public class CrearCuentaCorrienteDto
    {
        public int PersonaId { get; set; }
        public DateTime? FechaApertura { get; set; }
        public decimal SaldoApertura { get; set; } = 0m;
        public bool Activa { get; set; } = true;
        public decimal? LimiteCredito { get; set; }

    }
}
