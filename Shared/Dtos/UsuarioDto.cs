using IurixBlazor.Shared.Enums;

namespace IurixBlazor.Shared.Dtos

{ 


    public class UsuarioDto
    {
        public int Id { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;

        public string? Password { get; set; } = string.Empty;

        //public string Rol { get; set; } = "Usuario";

        public int RolId { get; set; }
        public string RolNombre { get; set; } = string.Empty;
        public List<string> Permisos { get; set; } = new();

        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;

        public Cargo Cargo { get; set; } = Cargo.Abogado; // Valor por defecto, si querés

        public string Matricula { get; set; } = string.Empty;

        public string Matricula1 { get; set; } = string.Empty;

        public string Matricula2 { get; set; } = string.Empty;

        public string Matricula3 { get; set; } = string.Empty;


        public string email { get; set; } = string.Empty;

        public string Telefono { get; set; } = string.Empty;

        public string Celular { get; set; } = string.Empty;

        public string NombreCompleto => $"{Nombre} {Apellido}";

        // NUEVO
        public bool PuedeEditar { get; set; } = false;
        public bool PuedeEliminar { get; set; } = false;

        public string RazonSocial { get; set; } = string.Empty; //  Nuevo campo
        public string? DNI { get; set; } = string.Empty;
        public string? CUIT { get; set; } = string.Empty;
        public string? IIBB { get; set; } = string.Empty;


        public int? CondicionIvaId { get; set; }

        //  Campos derivados de la relación CondicionIva
        public string CondicionIvaNombre { get; set; } = string.Empty;
        public int CondicionIvaCodigoAfip { get; set; }

        //para factura y QR Factura
        public DateOnly? InicioActividades { get; set; }
        public string? DomicilioFiscal { get; set; }          // Domicilio fiscal del emisor
        public string? Provincia { get; set; }

    }

}