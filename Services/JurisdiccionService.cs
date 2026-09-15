using IurixBlazor.Pages;
using IurixBlazor.Services.Http;
using IurixBlazor.Shared.Dtos;
using System.Net.Http.Json;
using System.Text.Json;

namespace IurixBlazor.Services
{
    public class JurisdiccionService
    {
        private readonly IApiRequestSender _sender;

        public JurisdiccionService(IApiRequestSender sender)
        {
            //var baseUrl = $"{(config.UsaHttpsServidorBackend ? "https" : "http")}://{config.IpServidor}:{config.Puerto}/";
            //_httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _sender = sender;
        }

        public async Task<List<JurisdiccionDto>> ObtenerTodosAsync()
        {
            using var response = await _sender.SendAsync(HttpMethod.Get, "api/Jurisdiccion");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<JurisdiccionDto>>() ?? new();
        }

        public async Task<JurisdiccionDto?> ObtenerPorIdAsync(int id)
        {
            using var resp = await _sender.SendAsync(HttpMethod.Get, $"api/Jurisdiccion/{id}");
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<JurisdiccionDto>();
        }

        public async Task CrearAsync(CrearJurisdiccionDto dto)
        {
            using var response = await _sender.SendAsync(HttpMethod.Post, "api/Jurisdiccion", JsonContent.Create(dto));
        }
        public async Task ActualizarAsync(int id, CrearJurisdiccionDto dto)
        {
            using var response = await _sender.SendAsync(HttpMethod.Patch, $"api/Jurisdiccion/{id}", JsonContent.Create(dto));
        }
        public async Task EliminarAsync(int id)
        {
            using var response = await _sender.SendAsync(HttpMethod.Delete, $"api/Jurisdiccion/{id}");
        }
    }
}
