using System.ComponentModel.DataAnnotations;

namespace IurixBlazor.Shared.Dtos
{
    public class ActualizarEstudioConfigDto
    {
        public string? Nombre { get; set; }
        public string? Domicilio { get; set; }
        public string? Telefono { get; set; }
        [EmailAddress] public string? Email { get; set; }
        [Url] public string? Web { get; set; }
        public string? LogoBase64 { get; set; }
        public string? Cuit { get; set; }
        public string? Provincia { get; set; }
        public string? Localidad { get; set; }
        public string? Observaciones { get; set; }
    }
}
