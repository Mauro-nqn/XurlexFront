namespace IurixBlazor.Shared.Dtos
{
    public class PresupuestoDetallePatchDto
    {
        public int? Id { get; set; }
        public string? Descripcion { get; set; }
        public decimal? Cantidad { get; set; }
        public decimal? PrecioUnitario { get; set; }

        public int IvaAlicuotaId { get; set; }  //  NUEVO
        public int? AjusteVariableId { get; set; }

        public decimal? VariableCoef { get; set; }
        public decimal? VariableValorUsado { get; set; }

    }
}
