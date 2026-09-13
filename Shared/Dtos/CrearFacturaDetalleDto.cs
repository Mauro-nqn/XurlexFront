namespace IurixBlazor.Shared.Dtos
{
    public class CrearFacturaDetalleDto
    {
        public string? Descripcion { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public int IvaAlicuotaId { get; set; }

        // Solo para “arrastrar” el vínculo desde el front (no se persiste en FacturaDetalle)
        public int? PresupuestoDetalleId { get; set; }
    }
}
