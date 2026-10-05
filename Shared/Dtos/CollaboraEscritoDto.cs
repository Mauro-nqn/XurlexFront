namespace IurixBlazor.Shared.Dtos
{
    public class CollaboraEscritoDto
    {
        public int Id { get; set; }

        public string Titulo { get; set; } = string.Empty;

        public DateTime FechaCreacion { get; set; }

        public string Formato { get; set; } = string.Empty;
    }
}