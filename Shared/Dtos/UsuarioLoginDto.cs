namespace IurixBlazor.Shared.Dtos
{
    public class UsuarioLoginDto
    {
        public string? NombreUsuario { get; set; }
        public string? Password { get; set; }
        public string? ClaveLicencia { get; set; }
        public string? DispositivoId { get; set; }

        public int IdLicencia { get; set; }


        public string DispositivoSesionId { get; set; } = string.Empty; // nuevo ID para sesiones
        public string NombreDispositivo { get; set; } = string.Empty;   // opcional, más legible

        public string TipoConexion { get; set; } = "Local"; // ?? NUEVO CAMPO

        public List<string> Permisos { get; set; } = new();


    }
}