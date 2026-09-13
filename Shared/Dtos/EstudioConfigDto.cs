namespace IurixBlazor.Shared.Dtos
{
    public class EstudioConfigDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string? Domicilio { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? Web { get; set; }
        public string? LogoBase64 { get; set; }
        public string? Cuit { get; set; }
        public string? Provincia { get; set; }
        public string? Localidad { get; set; }
        public string? Observaciones { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
