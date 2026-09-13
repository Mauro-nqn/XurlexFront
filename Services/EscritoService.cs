using IurixBlazor.Pages;
using IurixBlazor.Shared.Config;
using IurixBlazor.Shared.Dtos;
using System.Net.Http.Json;

namespace IurixBlazor.Shared.Services
{
    public class EscritoService
    {
        private readonly HttpClient _httpClient;

        public EscritoService(IHttpClientFactory factory, ConfigService config)
        {
            //var baseUrl = $"{(config.UsaHttpsServidorBackend ? "https" : "http")}://{config.IpServidor}:{config.Puerto}/";
            //_httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _httpClient = factory.CreateClient("Api");
        }

        public async Task<ResultadoPaginadoDto<EscritoDto>> ObtenerEscritosAsync(int page = 1, int pageSize = 20, string? filtro = null)
        {
            var query = $"api/escritos/listar?page={page}&pageSize={pageSize}";
            if (!string.IsNullOrWhiteSpace(filtro))
                query += $"&filtro={Uri.EscapeDataString(filtro)}";

            var response = await _httpClient.GetAsync(query);
            if (!response.IsSuccessStatusCode) return new();

            return await response.Content.ReadFromJsonAsync<ResultadoPaginadoDto<EscritoDto>>() ?? new();
        }

        public async Task<EscritoDto?> ObtenerEscritoPorIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/escritos/{id}");
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<EscritoDto>() : null;
        }

        public async Task<bool> GuardarEscritoAsync(string titulo, string html)
        {
            var dto = new { titulo, contenidoHtml = html };
            var response = await _httpClient.PostAsJsonAsync("api/escritos/guardar", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ActualizarEscritoAsync(int id, string titulo, string html)
        {
            var dto = new { titulo, contenidoHtml = html };
            var response = await _httpClient.PatchAsJsonAsync($"api/escritos/{id}", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> EliminarEscritoAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/escritos/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
