namespace IurixBlazor.Shared.Dtos
{
    public class PresupuestoDetalleDto
    {
        public int Id { get; set; }
        public int PresupuestoId { get; set; }
        public string? Descripcion { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public int IvaAlicuotaId { get; set; }
        //public decimal Subtotal => Cantidad * PrecioUnitario;

        public int? AjusteVariableId { get; set; }

        public decimal? VariableCoef { get; set; }
        public decimal? VariableValorUsado { get; set; }

        // opcionales para la vista
        public string? AjusteVariableClave { get; set; }
        public decimal? AjusteVariableValorActual { get; set; }


        public decimal Subtotal => Math.Round(Cantidad * PrecioUnitario, 2, MidpointRounding.AwayFromZero);
    }
}
