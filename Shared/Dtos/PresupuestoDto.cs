using IurixBlazor.Shared.Enums;

namespace IurixBlazor.Shared.Dtos
{
    public class PresupuestoDto
    {
        public string Nombre { get; set; } = string.Empty;
        public int Id { get; set; }
        public int PersonaId { get; set; }
        public PersonaDto? Persona { get; set; }

        public decimal Neto { get; set; }
        public decimal Iva { get; set; }
        public decimal Total { get; set; }

        public EstadoPresupuesto Estado { get; set; } = EstadoPresupuesto.Presupuestado;
        public DateTime Fecha { get; set; } = DateTime.Now;

        public int DistribucionHonorarioId { get; set; }
        public DistribucionHonorarioDto? DistribucionHonorario { get; set; }

        public List<PresupuestoDetalleDto> Detalles { get; set; } = new();

        public int? PlazoValidezDias { get; set; } = 15;
        public string? Observaciones { get; set; }
        public bool? MarcarEnviado { get; set; } // si lo tildás al crear

        public DateTime? FechaEnvio { get; set; }

        public AjusteEstadoPresupuesto AjusteEstado { get; set; }

        public DateTimeOffset? RevalorizadoUtc { get; set; }
    }
}
