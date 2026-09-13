using IurixBlazor.Pages;
using IurixBlazor.Shared.Config;
using IurixBlazor.Shared.Dtos;
using System.Net.Http.Json;

namespace IurixBlazor.Services
{
    public class SecretariaService
    {
        private readonly HttpClient _httpClient;

        public SecretariaService(IHttpClientFactory factory, ConfigService config)
        {
            //var baseUrl = $"{(config.UsaHttpsServidorBackend ? "https" : "http")}://{config.IpServidor}:{config.Puerto}/";
            //_httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _httpClient = factory.CreateClient("Api");
        }

        public async Task<List<SecretariaDto>> ObtenerTodosAsync()
            => await _httpClient.GetFromJsonAsync<List<SecretariaDto>>("api/Secretaria") ?? new();

        public async Task<SecretariaDto?> ObtenerPorIdAsync(int id)
            => await _httpClient.GetFromJsonAsync<SecretariaDto>($"api/Secretaria/{id}");

        public async Task<List<SecretariaDto>> ObtenerPorJuzgadoAsync(int juzgadoId)
        {
            var response = await _httpClient.GetAsync($"api/Secretaria/por-juzgado/{juzgadoId}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return new List<SecretariaDto>(); // Si no hay resultados, devolvemos lista vacía

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<SecretariaDto>>() ?? new();
        }

        public async Task CrearAsync(CrearSecretariaDto dto)
            => await _httpClient.PostAsJsonAsync("api/Secretaria", dto);

        public async Task ActualizarAsync(int id, CrearSecretariaDto dto)
            => await _httpClient.PatchAsync($"api/Secretaria/{id}", JsonContent.Create(dto));

        public async Task EliminarAsync(int id)
            => await _httpClient.DeleteAsync($"api/Secretaria/{id}");
    }
}
