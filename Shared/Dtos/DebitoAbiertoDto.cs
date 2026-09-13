namespace IurixBlazor.Shared.Dtos
{
    public class DebitoAbiertoDto
    {
        public MovimientoCuentaDto Debito { get; set; } = new();
        public decimal Saldo { get; set; }
    }
}
