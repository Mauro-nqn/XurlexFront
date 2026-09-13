namespace IurixBlazor.Shared.Dtos
{
    public class FacturaDetalleDto
    {
        public int Id { get; set; }
        public int FacturaId { get; set; }
        public string? Descripcion { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public int IvaAlicuotaId { get; set; }


        public decimal IvaPorcentaje { get; set; }  //  Nuevo


        public decimal TotalConIva => Cantidad * PrecioUnitario * (1 + (IvaPorcentaje / 100));

        // Baseline del presupuesto (para cálculo en front)  
        public int? PresupuestoDetalleId { get; set; }              
        public bool EsDePresupuesto => PresupuestoDetalleId.HasValue;

        // Baselines (ya los usás)
        public decimal? PrecioPresNeto { get; set; }
        public decimal? PrecioPresFinal { get; set; }
        public decimal TasaIvaPres { get; set; }
        public decimal CantidadPendiente { get; set; }

        // Para un eventual “desbloquear precio/IVA”
        public bool PermitirRenegociar { get; set; } = false;


       

        // Baselines para renegociar manteniendo el importe:
        public decimal? ImporteObjetivoFinal { get; set; }  // Cantidad * PrecioFinal al desbloquear
        public decimal? CantidadBase { get; set; }          // snapshot al desbloquear (opcional, debug)
        public decimal? PrecioBase { get; set; }            // snapshot al desbloquear (opcional, debug)

        


        //  Propiedad calculada TotalConIva - con iva harcodeado
        //public decimal TotalConIva
        //{
        //    get
        //    {
        //        decimal porcentajeIva = IvaAlicuotaId switch
        //        {
        //            1 => 0.21m,   // 21%
        //            2 => 0.105m,  // 10.5%
        //            3 => 0.00m,   // 0%
        //            _ => 0.00m
        //        };
        //        return Cantidad * PrecioUnitario * (1 + porcentajeIva);
        //    }
        //}
    }
}
