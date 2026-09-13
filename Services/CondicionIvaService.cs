using IurixBlazor.Pages;
using IurixBlazor.Shared.Config;
using IurixBlazor.Shared.Dtos;
using System.Net.Http.Json;
using System.Text.Json;

namespace IurixBlazor.Services
{
    public class CondicionIvaService
    {
        private readonly HttpClient _httpClient;

        public CondicionIvaService(IHttpClientFactory factory, ConfigService config)
        {
            //var baseUrl = $"{(config.UsaHttpsServidorBackend ? "https" : "http")}://{config.IpServidor}:{config.Puerto}/";
            //_httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _httpClient = factory.CreateClient("Api");
        }

        /// <summary>
        /// Obtiene todas las condiciones de IVA
        /// </summary>
        public async Task<List<CondicionIvaDto>> ObtenerTodasAsync()
        {
            var response = await _httpClient.GetAsync("api/CondicionIva");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<CondicionIvaDto>>() ?? new();
        }

        /// <summary>
        /// Obtiene una condición de IVA por ID
        /// </summary>
        public async Task<CondicionIvaDto?> ObtenerPorIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/CondicionIva/{id}");
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<CondicionIvaDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        /// <summary>
        /// Crea una nueva condición de IVA
        /// </summary>
        public async Task CrearAsync(CondicionIvaDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/CondicionIva", dto);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Actualiza una condición de IVA existente
        /// </summary>
        public async Task ActualizarAsync(int id, CondicionIvaDto dto)
        {
            var response = await _httpClient.PatchAsync($"api/CondicionIva/{id}", JsonContent.Create(dto));
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Elimina una condición de IVA por ID
        /// </summary>
        public async Task EliminarAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/CondicionIva/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}
