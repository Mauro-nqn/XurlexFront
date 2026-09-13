namespace IurixBlazor.Shared.Dtos
{
    public class RubroDto
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }
        public string? Tipo { get; set; }
        public bool Activo { get; set; } = true;
    }
}
