namespace IurixBlazor.Shared.Dtos
{
    public class PendientePresupuestoDto
    {
        public int PresupuestoId { get; set; }
        public decimal Total { get; set; }
        public decimal YaFacturado { get; set; }
        public decimal Pendiente { get; set; }
    }
}
