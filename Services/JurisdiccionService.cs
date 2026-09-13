using IurixBlazor.Pages;
using IurixBlazor.Shared.Config;
using IurixBlazor.Shared.Dtos;
using System.Net.Http.Json;
using System.Text.Json;

namespace IurixBlazor.Services
{
    public class JurisdiccionService
    {
        private readonly HttpClient _httpClient;

        public JurisdiccionService(IHttpClientFactory factory, ConfigService config)
        {
            //var baseUrl = $"{(config.UsaHttpsServidorBackend ? "https" : "http")}://{config.IpServidor}:{config.Puerto}/";
            //_httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _httpClient = factory.CreateClient("Api");
        }

        public async Task<List<JurisdiccionDto>> ObtenerTodosAsync()
            => await _httpClient.GetFromJsonAsync<List<JurisdiccionDto>>("api/Jurisdiccion") ?? new();

        public async Task<JurisdiccionDto?> ObtenerPorIdAsync(int id)
        {
            var resp = await _httpClient.GetAsync($"api/Jurisdiccion/{id}");
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<JurisdiccionDto>();
        }

        public async Task CrearAsync(CrearJurisdiccionDto dto) => await _httpClient.PostAsJsonAsync("api/Jurisdiccion", dto);
        public async Task ActualizarAsync(int id, CrearJurisdiccionDto dto) => await _httpClient.PatchAsync($"api/Jurisdiccion/{id}", JsonContent.Create(dto));
        public async Task EliminarAsync(int id) => await _httpClient.DeleteAsync($"api/Jurisdiccion/{id}");
    }
}
