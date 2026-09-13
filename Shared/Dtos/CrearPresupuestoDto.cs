using IurixBlazor.Shared.Enums;

namespace IurixBlazor.Shared.Dtos
{
    public class CrearPresupuestoDto
    {
        public string Nombre { get; set; } = string.Empty;
        public int PersonaId { get; set; }
        public decimal Neto { get; set; }
        public decimal Iva { get; set; }
        public decimal Total { get; set; }
        public EstadoPresupuesto Estado { get; set; }
        public DateTime Fecha { get; set; }
        public int DistribucionHonorarioId { get; set; }
        public List<CrearPresupuestoDetalleDto> Detalles { get; set; } = new();

        public int? PlazoValidezDias { get; set; } = 15;
        public string? Observaciones { get; set; }
        public bool? MarcarEnviado { get; set; } // si lo tildás al crear

        public DateTime? FechaEnvio { get; set; }
    }
}
