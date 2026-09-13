namespace IurixBlazor.Shared.Dtos
{
    /// <summary>
    /// DTO auxiliar para respuesta de autenticación.
    /// </summary>
    public class AuthResponseDto
    {
        public string TA { get; set; } = string.Empty;
        public string Sign { get; set; } = string.Empty;
        public DateTime TokenExpiration { get; set; }
    }
}

