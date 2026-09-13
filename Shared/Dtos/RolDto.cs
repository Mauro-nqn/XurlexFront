namespace IurixBlazor.Shared.Dtos
{
    public class RolDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public List<string> Permisos { get; set; } = new();
    }
}
