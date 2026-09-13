namespace IurixBlazor.Shared.Dtos
{
    public class CrearDomicilioDto
    {
        public string Descripcion { get; set; } = string.Empty;

        public TipoDomicilio Tipo { get; set; }

        public string? Ciudad { get; set; }
        public string? Provincia { get; set; }
    }
}
