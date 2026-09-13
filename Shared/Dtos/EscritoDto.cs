namespace IurixBlazor.Shared.Dtos
{
    public class EscritoDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public string ContenidoHtml { get; set; } = string.Empty;
    }
}
