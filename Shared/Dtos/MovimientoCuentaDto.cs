using IurixBlazor.Shared.Enums;

namespace IurixBlazor.Shared.Dtos
{
    public class MovimientoCuentaDto
    {
        public int Id { get; set; }
        public int? CuentaCorrienteId { get; set; }
        public DateTime? Fecha { get; set; }
        public TipoMovimientoCuenta? Tipo { get; set; }
        public decimal? Importe { get; set; }
        public string? Detalle { get; set; }
        public OrigenMovimientoCuenta? OrigenTipo { get; set; }
        public int? OrigenId { get; set; }
        public int? PresupuestoId { get; set; }
        public int? FacturaId { get; set; }
        public int? GestionId { get; set; }
        public int? CajaMovimientoId { get; set; }
        public int? RubroId { get; set; }
        public int? CentroCostoId { get; set; }
        public int? UsuarioId { get; set; }
        public decimal? SaldoLuego { get; set; }
    }
}
