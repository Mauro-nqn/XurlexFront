namespace IurixBlazor.Shared.Dtos

{

    public class LoginResponseDto
    {
        public string? Token { get; set; }
        public UsuarioDto? Usuario { get; set; }
        public LicenciaDto? Licencia { get; set; }
        public DateTime ExpiracionToken { get; set; }
    }


}
