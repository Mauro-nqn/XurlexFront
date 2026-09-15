using IurixBlazor.Pages;
using IurixBlazor.Services.Http;
using IurixBlazor.Shared.Dtos;
using System.Net.Http.Json;

namespace IurixBlazor.Services
{
    public class CircunscripcionService
    {
        private readonly IApiRequestSender _sender;

        public CircunscripcionService(IApiRequestSender sender)
        {
            //var baseUrl = $"{(config.UsaHttpsServidorBackend ? "https" : "http")}://{config.IpServidor}:{config.Puerto}/";
            //_httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _sender = sender;
        }

        public async Task<List<CircunscripcionDto>> ObtenerTodosAsync()
        {
            using var response = await _sender.SendAsync(HttpMethod.Get, "api/Circunscripcion");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<CircunscripcionDto>>() ?? new();
        }

        public async Task<CircunscripcionDto?> ObtenerPorIdAsync(int id)
        {
            using var response = await _sender.SendAsync(HttpMethod.Get, $"api/Circunscripcion/{id}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<CircunscripcionDto>();
        }

        public async Task CrearAsync(CrearCircunscripcionDto dto)
        {
            using var response = await _sender.SendAsync(HttpMethod.Post, "api/Circunscripcion", JsonContent.Create(dto));
        }

        public async Task ActualizarAsync(int id, CrearCircunscripcionDto dto)
        {
            using var response = await _sender.SendAsync(HttpMethod.Patch, $"api/Circunscripcion/{id}", JsonContent.Create(dto));
        }

        public async Task EliminarAsync(int id)
        {
            using var response = await _sender.SendAsync(HttpMethod.Delete, $"api/Circunscripcion/{id}");
        }


        public async Task<List<CircunscripcionDto>> ObtenerPorJurisdiccionAsync(int jurisdiccionId)
        {
            using var response = await _sender.SendAsync(HttpMethod.Get, $"api/Circunscripcion/por-jurisdiccion/{jurisdiccionId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<CircunscripcionDto>>() ?? new();
        }
    }
}
