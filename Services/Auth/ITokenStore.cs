namespace IurixBlazor.Services.Auth
{
    public interface ITokenStore
    {
        Task SetAsync(string? token, string? refreshToken = null, DateTimeOffset? exp = null);
        Task<string?> GetAsync();
        Task ClearAsync();
    }
}

