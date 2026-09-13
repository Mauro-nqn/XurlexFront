using IurixBlazor.Shared.Enums;

namespace IurixBlazor.Shared.Dtos
{
    public class PersonaDto
    {
        //public string Tipo { get; set; } = string.Empty;

        public int Id { get; set; }
        public TipoPersona Tipo { get; set; }
        public string Apellido { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public int? ClasificacionId { get; set; }
        
        public string DNI { get; set; } = string.Empty;
        public string Nacionalidad { get; set; } = string.Empty;
        public DateOnly? FechaNacimiento { get; set; }
        public string EstadoCivil { get; set; } = string.Empty;
        public string CUIT { get; set; } = string.Empty;
        public string RazonSocial { get; set; } = string.Empty;
        public int? CondicionIVAId { get; set; }

        public string CondicionIvaNombre { get; set; } = string.Empty; //  NUEVO

        public int? CondicionIvaCodigoAfip { get; set; }

        public string CondicionIIBB { get; set; } = string.Empty;

        public string Domicilio { get; set; } = string.Empty;
        public string Ciudad { get; set; } = string.Empty;
        public string CodigoPostal { get; set; } = string.Empty;
        public string Provincia { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Celular { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string Observacion { get; set; } = string.Empty;

    }
}
