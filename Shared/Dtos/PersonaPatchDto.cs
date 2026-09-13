namespace IurixBlazor.Shared.Dtos
{
    public class PersonaPatchDto
    {
        public string? Tipo { get; set; }
        public string? Apellido { get; set; }
        public string? Nombre { get; set; }
        public int? ClasificacionId { get; set; }
        
        public string? DNI { get; set; }
        public string? Nacionalidad { get; set; }
        public DateOnly? FechaNacimiento { get; set; }
        public string? EstadoCivil { get; set; }
        public string? CUIT { get; set; }
        public string? RazonSocial { get; set; }
        public int? CondicionIVAId { get; set; }
        public string? CondicionIIBB { get; set; }
        public string? Domicilio { get; set; }
        public string? Ciudad { get; set; }
        public string? CodigoPostal { get; set; }
        public string? Provincia { get; set; }
        public string? Telefono { get; set; }
        public string? Celular { get; set; }
        public string? Email { get; set; }

        public string? Observacion { get; set; }
    }
}
