using IurixBlazor.Pages;
using IurixBlazor.Shared.Config;
using IurixBlazor.Shared.Dtos;
using System.Net.Http.Json;
using System.Text.Json;

namespace IurixBlazor.Services
{
    public class IvaAlicuotaService
    {
        private readonly HttpClient _httpClient;

        public IvaAlicuotaService(IHttpClientFactory factory, ConfigService config)
        {
            //var baseUrl = $"{(config.UsaHttpsServidorBackend ? "https" : "http")}://{config.IpServidor}:{config.Puerto}/";
            //_httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _httpClient = factory.CreateClient("Api");
        }

        public async Task<List<IvaAlicuotaDto>> ObtenerTodasAsync()
        {
            var response = await _httpClient.GetAsync("api/IvaAlicuota");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<IvaAlicuotaDto>>() ?? new();
        }

        public async Task<IvaAlicuotaDto?> ObtenerPorIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/IvaAlicuota/{id}");
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IvaAlicuotaDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        //Obtener por CodigoAfip
        public async Task<IvaAlicuotaDto?> ObtenerPorCodigoAfipAsync(int codigoAfip)
        {
            return await _httpClient.GetFromJsonAsync<IvaAlicuotaDto>($"api/ivAlicuota/codigoAfip/{codigoAfip}");
        }

        public async Task CrearAsync(IvaAlicuotaDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/IvaAlicuota", dto);
            response.EnsureSuccessStatusCode();
        }

        public async Task ActualizarAsync(int id, IvaAlicuotaDto dto)
        {
            var response = await _httpClient.PatchAsync($"api/IvaAlicuota/{id}", JsonContent.Create(dto));
            response.EnsureSuccessStatusCode();
        }

        public async Task EliminarAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/IvaAlicuota/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}
