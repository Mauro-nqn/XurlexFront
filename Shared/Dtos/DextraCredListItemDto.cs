namespace IurixBlazor.Shared.Dtos
{
    public class DextraCredListItemDto
    {
        public int Id { get; set; }
        public string Host { get; set; } = "";
        public string UsuarioDextra { get; set; } = "";
        public bool IsDefault { get; set; }
        public DateTimeOffset? LastVerifiedAt { get; set; }
        public bool? LastVerifyOk { get; set; }
        public bool HasPassword { get; set; } // nunca devolvemos el password
    }
}
