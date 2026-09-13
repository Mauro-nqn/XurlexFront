namespace IurixBlazor.Shared.Dtos
{
    public class DextraCredUpsertDto
    {
        public string? Host { get; set; }         // null = mantener
        public string? UsuarioDextra { get; set; }// null = mantener
        public string? PasswordPlain { get; set; }// null/"" = no cambiar
        public bool? IsDefault { get; set; }      // opcional
    }
}
