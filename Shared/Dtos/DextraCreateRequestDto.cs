namespace IurixBlazor.Shared.Dtos
{
    public class DextraCreateRequestDto
    {
        public int? UsuarioId { get; set; }       // opcional, el backend usa route {userId}
        public string Host { get; set; } = "";    // puede ir vacío y backend pondrá el default
        public string UsuarioDextra { get; set; } = "";
        public string PasswordPlain { get; set; } = "";
        public bool IsDefault { get; set; } = true;
    }
}
