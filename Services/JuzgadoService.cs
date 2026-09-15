using IurixBlazor.Pages;
using IurixBlazor.Services.Http;
using IurixBlazor.Shared.Dtos;
using System.Net.Http.Json;

namespace IurixBlazor.Services
{
    public class JuzgadoService
    {
        private readonly IApiRequestSender _sender;

        public JuzgadoService(IApiRequestSender sender)
        {
            //var baseUrl = $"{(config.UsaHttpsServidorBackend ? "https" : "http")}://{config.IpServidor}:{config.Puerto}/";
            //_httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _sender = sender;
        }

        public async Task<List<JuzgadoDto>> ObtenerTodosAsync()
        {
            using var response = await _sender.SendAsync(HttpMethod.Get, "api/Juzgado");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<JuzgadoDto>>() ?? new();
        }

        public async Task<JuzgadoDto?> ObtenerPorIdAsync(int id)
        {
            using var response = await _sender.SendAsync(HttpMethod.Get, $"api/Juzgado/{id}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<JuzgadoDto>();
        }

        public async Task CrearAsync(CrearJuzgadoDto dto)
        {
            using var response = await _sender.SendAsync(HttpMethod.Post, "api/Juzgado", JsonContent.Create(dto));
        }

        public async Task ActualizarAsync(int id, CrearJuzgadoDto dto)
        {
            using var response = await _sender.SendAsync(HttpMethod.Patch, $"api/Juzgado/{id}", JsonContent.Create(dto));
        }

        public async Task EliminarAsync(int id)
        {
            using var response = await _sender.SendAsync(HttpMethod.Delete, $"api/Juzgado/{id}");
        }

        public async Task<List<JuzgadoDto>> ObtenerPorCircunscripcionAsync(int circunscripcionId)
        {
            using var response = await _sender.SendAsync(HttpMethod.Get, $"api/Juzgado/por-circunscripcion/{circunscripcionId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<JuzgadoDto>>() ?? new();
        }

    }
}
