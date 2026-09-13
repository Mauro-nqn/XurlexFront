namespace IurixBlazor.Shared.Dtos
{
    public class CrearEventoDto
    {
        public string Titulo { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
    }
}
