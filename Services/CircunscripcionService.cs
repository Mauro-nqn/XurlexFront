using IurixBlazor.Pages;
using IurixBlazor.Shared.Config;
using IurixBlazor.Shared.Dtos;
using System.Net.Http.Json;

namespace IurixBlazor.Services
{
    public class CircunscripcionService
    {
        private readonly HttpClient _httpClient;

        public CircunscripcionService(IHttpClientFactory factory, ConfigService config)
        {
            //var baseUrl = $"{(config.UsaHttpsServidorBackend ? "https" : "http")}://{config.IpServidor}:{config.Puerto}/";
            //_httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _httpClient = factory.CreateClient("Api");
        }

        public async Task<List<CircunscripcionDto>> ObtenerTodosAsync()
            => await _httpClient.GetFromJsonAsync<List<CircunscripcionDto>>("api/Circunscripcion") ?? new();

        public async Task<CircunscripcionDto?> ObtenerPorIdAsync(int id)
            => await _httpClient.GetFromJsonAsync<CircunscripcionDto>($"api/Circunscripcion/{id}");

        public async Task CrearAsync(CrearCircunscripcionDto dto)
            => await _httpClient.PostAsJsonAsync("api/Circunscripcion", dto);

        public async Task ActualizarAsync(int id, CrearCircunscripcionDto dto)
            => await _httpClient.PatchAsync($"api/Circunscripcion/{id}", JsonContent.Create(dto));

        public async Task EliminarAsync(int id)
            => await _httpClient.DeleteAsync($"api/Circunscripcion/{id}");


        public async Task<List<CircunscripcionDto>> ObtenerPorJurisdiccionAsync(int jurisdiccionId)
        {
            var response = await _httpClient.GetAsync($"api/Circunscripcion/por-jurisdiccion/{jurisdiccionId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<CircunscripcionDto>>() ?? new();
        }
    }
}
