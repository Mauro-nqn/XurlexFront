namespace IurixBlazor.Shared.Dtos
{
    public class HonorarioMensualProfesionalDto
    {
        public int Anio { get; set; }
        public int Mes { get; set; }
        public int? AbogadoId { get; set; }
        public string AbogadoNombre { get; set; } = string.Empty;
        public decimal TotalHonorarios { get; set; }

        public string MesTexto => new DateTime(Anio, Mes, 1).ToString("MMMM yyyy");
    }
}
