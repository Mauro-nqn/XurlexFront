namespace IurixBlazor.Shared.Dtos
{
    public class DatosAfipDto
    {
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? RazonSocial { get; set; }
        public string? Direccion { get; set; }
        public string? CodPostal { get; set; }
        public string? DescripcionProvincia { get; set; }
        public string? Localidad { get; set; }
        public int IdImpuesto { get; set; }  // Código AFIP de la condición IVA

        public string? TipoPersona { get; set; }
    }
}
