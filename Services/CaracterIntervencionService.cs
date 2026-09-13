using IurixBlazor.Pages;
using IurixBlazor.Shared.Config;
using IurixBlazor.Shared.Dtos;
using System.Net.Http.Json;

namespace IurixBlazor.Services
{
    public class CaracterIntervencionService
    {
        private readonly HttpClient _httpClient;

        public CaracterIntervencionService(IHttpClientFactory factory, ConfigService config)
        {
            //var baseUrl = $"{(config.UsaHttpsServidorBackend ? "https" : "http")}://{config.IpServidor}:{config.Puerto}/";
            //_httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _httpClient = factory.CreateClient("Api");
        }

        public async Task<List<CaracterIntervencionDto>> ObtenerTodosAsync()
            => await _httpClient.GetFromJsonAsync<List<CaracterIntervencionDto>>("api/CaracterIntervencion") ?? new();

        public async Task<CaracterIntervencionDto?> ObtenerPorIdAsync(int id)
            => await _httpClient.GetFromJsonAsync<CaracterIntervencionDto>($"api/CaracterIntervencion/{id}");

        public async Task CrearAsync(CaracterIntervencionDto dto)
            => await _httpClient.PostAsJsonAsync("api/CaracterIntervencion", dto);

        public async Task ActualizarAsync(int id, CaracterIntervencionDto dto)
            => await _httpClient.PatchAsync($"api/CaracterIntervencion/{id}", JsonContent.Create(dto));

        public async Task EliminarAsync(int id)
            => await _httpClient.DeleteAsync($"api/CaracterIntervencion/{id}");
    }
}
