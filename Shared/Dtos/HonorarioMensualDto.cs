namespace IurixBlazor.Shared.Dtos
{
    public class HonorarioMensualDto
    {
        public int Anio { get; set; }
        public int Mes { get; set; }
        public decimal TotalHonorarios { get; set; }

        public string MesTexto => new DateTime(Anio, Mes, 1).ToString("MMMM yyyy");
    }
}
