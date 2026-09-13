using IurixBlazor.Pages;
using IurixBlazor.Shared.Config;
using IurixBlazor.Shared.Dtos;
using System.Net.Http.Json;

namespace IurixBlazor.Services
{
    public class JuzgadoService
    {
        private readonly HttpClient _httpClient;

        public JuzgadoService(IHttpClientFactory factory, ConfigService config)
        {
            //var baseUrl = $"{(config.UsaHttpsServidorBackend ? "https" : "http")}://{config.IpServidor}:{config.Puerto}/";
            //_httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _httpClient = factory.CreateClient("Api");
        }

        public async Task<List<JuzgadoDto>> ObtenerTodosAsync()
            => await _httpClient.GetFromJsonAsync<List<JuzgadoDto>>("api/Juzgado") ?? new();

        public async Task<JuzgadoDto?> ObtenerPorIdAsync(int id)
            => await _httpClient.GetFromJsonAsync<JuzgadoDto>($"api/Juzgado/{id}");

        public async Task CrearAsync(CrearJuzgadoDto dto)
            => await _httpClient.PostAsJsonAsync("api/Juzgado", dto);

        public async Task ActualizarAsync(int id, CrearJuzgadoDto dto)
            => await _httpClient.PatchAsync($"api/Juzgado/{id}", JsonContent.Create(dto));

        public async Task EliminarAsync(int id)
            => await _httpClient.DeleteAsync($"api/Juzgado/{id}");

        public async Task<List<JuzgadoDto>> ObtenerPorCircunscripcionAsync(int circunscripcionId)
        {
            var response = await _httpClient.GetAsync($"api/Juzgado/por-circunscripcion/{circunscripcionId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<JuzgadoDto>>() ?? new();
        }

    }
}
