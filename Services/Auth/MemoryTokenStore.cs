namespace IurixBlazor.Services.Auth
{
    public class MemoryTokenStore : ITokenStore
    {
        private string? _token;
        private readonly object _lock = new();

        public Task SetAsync(string? token, string? refreshToken = null, DateTimeOffset? exp = null)
        {
            lock (_lock) { _token = token; }
            return Task.CompletedTask;
        }

        public Task<string?> GetAsync()
        {
            lock (_lock) { return Task.FromResult(_token); }
        }

        public Task ClearAsync()
        {
            lock (_lock) { _token = null; }
            return Task.CompletedTask;
        }
    }
}
