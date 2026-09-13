using System.Text.Json.Serialization;

namespace IurixBlazor.Shared.Dtos
{
    public class GoogleAuthDto
    {
        [JsonPropertyName("accessToken")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("refreshToken")]
        public string? RefreshToken { get; set; }

        [JsonPropertyName("expira")]
        public DateTimeOffset? Expira { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }
    }
}
