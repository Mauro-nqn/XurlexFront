namespace IurixBlazor.Shared.Dtos
{
    public class InformeHonorariosFiltroDto
    {
        public DateTime FechaDesde { get; set; }
        public DateTime FechaHasta { get; set; }

        public int? PersonaId { get; set; }   // opcional, filtrar por cliente
        public int? AbogadoId { get; set; }   // opcional, filtrar por profesional

        public bool IncluirIngresosSinImputar { get; set; } = true;
    }
}
