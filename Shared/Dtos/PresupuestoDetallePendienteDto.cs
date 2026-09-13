namespace IurixBlazor.Shared.Dtos
{
    public class PresupuestoDetallePendienteDto
    {
        public int PresupuestoId { get; set; }
        public int PresupuestoDetalleId { get; set; }
        public string Descripcion { get; set; } = "";
        public decimal CantidadOriginal { get; set; }
        public decimal CantidadYaFacturada { get; set; }
        public decimal CantidadPendiente { get; set; }
        public decimal PrecioUnitarioPresupuesto { get; set; } // neto (A) o neto presupuestado; lo convertís en front si es B/C
        public int IvaAlicuotaId { get; set; }
    }
}
