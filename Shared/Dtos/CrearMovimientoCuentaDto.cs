using IurixBlazor.Shared.Enums;

namespace IurixBlazor.Shared.Dtos
{
    public class CrearMovimientoCuentaDto
    {
        public int CuentaCorrienteId { get; set; }
        public DateTime? Fecha { get; set; }
        public TipoMovimientoCuenta Tipo { get; set; }        // Debito|Credito
        public decimal Importe { get; set; }
        public string? Detalle { get; set; }
        public OrigenMovimientoCuenta OrigenTipo { get; set; }
        public int? OrigenId { get; set; }
        public int? PresupuestoId { get; set; }
        public int? FacturaId { get; set; }
        public int? GestionId { get; set; }
        public int? CajaMovimientoId { get; set; }
        public int? RubroId { get; set; }
        public int? CentroCostoId { get; set; }
        public int? UsuarioId { get; set; }
    }
}
