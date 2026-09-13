using IurixBlazor.Pages;
using IurixBlazor.Shared.Config;
using IurixBlazor.Shared.Dtos;
using System.Net.Http.Json;

namespace IurixBlazor.Services
{
    public class PuntoVentaService
    {
        private readonly HttpClient _httpClient;

        public PuntoVentaService(IHttpClientFactory factory, ConfigService config)
        {
            //var baseUrl = $"{(config.UsaHttpsServidorBackend ? "https" : "http")}://{config.IpServidor}:{config.Puerto}/";
            //_httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _httpClient = factory.CreateClient("Api");
        }

        public async Task<List<PuntoVentaDto>> ObtenerTodosAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<PuntoVentaDto>>("api/puntoventa") ?? new();
        }

        public async Task<PuntoVentaDto?> ObtenerPorIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<PuntoVentaDto>($"api/puntoventa/{id}");
        }

        public async Task CrearAsync(CrearPuntoVentaDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/puntoventa", dto);
            response.EnsureSuccessStatusCode();
        }

        //public async Task ActualizarAsync(int id, PuntoVentaDto dto)
        //{
        //    var response = await _httpClient.PatchAsJsonAsync($"api/puntoventa/{id}", dto);
        //    response.EnsureSuccessStatusCode();
        //}

        public async Task ActualizarAsync(int id, PuntoVentaDto dto)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(dto);
            System.Diagnostics.Debug.WriteLine($"📤 JSON enviado al backend: {json}");

            var response = await _httpClient.PatchAsJsonAsync($"api/puntoventa/{id}", dto);
            System.Diagnostics.Debug.WriteLine($"📥 Status Code devuelto: {response.StatusCode}");
            var content = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"📥 Contenido devuelto: {content}");

            response.EnsureSuccessStatusCode();
        }

        public async Task EliminarAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/puntoventa/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}
