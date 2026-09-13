namespace IurixBlazor.Shared.Dtos
{
    public class PresupuestoResumenDto
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Neto { get; set; }
        public decimal Iva { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; } = "";

        public string? Titulo { get; set; }
    }

    
}
