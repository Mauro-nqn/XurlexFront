using IurixBlazor.Pages;
using IurixBlazor.Shared.Config;
using IurixBlazor.Shared.Dtos;
using System.Net.Http.Json;

namespace IurixBlazor.Services
{
    public class TipoComprobanteService
    {
        private readonly HttpClient _httpClient;

        public TipoComprobanteService(IHttpClientFactory factory, ConfigService config)
        {
            //var baseUrl = $"{(config.UsaHttpsServidorBackend ? "https" : "http")}://{config.IpServidor}:{config.Puerto}/";
            //_httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _httpClient = factory.CreateClient("Api");
        }

        public async Task<List<TipoComprobanteDto>> ObtenerTodosAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<TipoComprobanteDto>>("api/tipocomprobante") ?? new();
        }

        public async Task<TipoComprobanteDto?> ObtenerPorCodigoAsync(int codigo)
        {
            return await _httpClient.GetFromJsonAsync<TipoComprobanteDto>($"api/tipocomprobante/{codigo}");
        }

        public async Task CrearAsync(CrearTipoComprobanteDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/tipocomprobante", dto);
            response.EnsureSuccessStatusCode();
        }

        public async Task ActualizarAsync(int codigo, CrearTipoComprobanteDto dto)
        {
            var response = await _httpClient.PatchAsJsonAsync($"api/tipocomprobante/{codigo}", dto);
            response.EnsureSuccessStatusCode();
        }

        public async Task EliminarAsync(int codigo)
        {
            var response = await _httpClient.DeleteAsync($"api/tipocomprobante/{codigo}");
            response.EnsureSuccessStatusCode();
        }
    }
}

