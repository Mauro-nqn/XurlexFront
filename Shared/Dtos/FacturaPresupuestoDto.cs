namespace IurixBlazor.Shared.Dtos
{
    public class FacturaPresupuestoDto
    {
        public int Id { get; set; }
        public int FacturaId { get; set; }
        public int PresupuestoId { get; set; }        
        public int? PresupuestoDetalleId { get; set; }

        public decimal CantidadFacturada { get; set; }          // NUEVO
        public decimal? PrecioUnitarioFacturado { get; set; }   // NUEVO (opcional)
        public decimal MontoFacturado { get; set; }
    }
}
