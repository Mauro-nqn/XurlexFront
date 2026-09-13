namespace IurixBlazor.Shared.Dtos
{
    public class CrearSecretariaDto
    {
        public string Nombre { get; set; } = string.Empty;
        public int JuzgadoId { get; set; }
        public string Juez { get; set; } = string.Empty;
        public string Secretario { get; set; } = string.Empty;
    }
}
