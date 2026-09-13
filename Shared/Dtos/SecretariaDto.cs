namespace IurixBlazor.Shared.Dtos
{
    public class SecretariaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int JuzgadoId { get; set; }
        public string JuzgadoNombre { get; set; } = string.Empty;
        public string Juez { get; set; } = string.Empty;
        public string Secretario { get; set; } = string.Empty;
    }
}
