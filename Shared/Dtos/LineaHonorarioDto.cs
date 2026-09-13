namespace IurixBlazor.Shared.Dtos
{
    public class LineaHonorarioDto
    {
        // Crédito (movimiento de cuenta)
        public int MovimientoCreditoId { get; set; }
        public DateTime FechaCredito { get; set; }

        // Recibo asociado (si existe)
        public int? ReciboId { get; set; }

        // Cliente
        public int PersonaId { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;

        public decimal ImporteCredito { get; set; }   // Importe del crédito total
        public decimal ImporteImputado { get; set; }  // Parte usada en esta imputación

        // Débito / Presupuesto
        public int? MovimientoDebitoId { get; set; }
        public int? PresupuestoId { get; set; }
        public string? NombrePresupuesto { get; set; }

        // Distribución
        public int? AbogadoId { get; set; }
        public string? AbogadoNombre { get; set; }
        public decimal? PorcentajeAbogado { get; set; } // 0–100

        public decimal ImporteParaAbogado { get; set; } // ImporteImputado * (Porcentaje/100)

        // Flags
        public bool EsIngresoSinImputar { get; set; }
        public bool EsSinDistribucion { get; set; }
    }
}
