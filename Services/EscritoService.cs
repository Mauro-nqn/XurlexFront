using IurixBlazor.Pages;
using IurixBlazor.Services.Http;
using IurixBlazor.Shared.Dtos;
using System.Net.Http.Json;

namespace IurixBlazor.Shared.Services
{
    public class EscritoService
    {
        private readonly IApiRequestSender _sender;

        public EscritoService(IApiRequestSender sender)
        {
            //var baseUrl = $"{(config.UsaHttpsServidorBackend ? "https" : "http")}://{config.IpServidor}:{config.Puerto}/";
            //_httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _sender = sender;
        }

        public async Task<ResultadoPaginadoDto<EscritoDto>> ObtenerEscritosAsync(int page = 1, int pageSize = 20, string? filtro = null)
        {
            var query = $"api/escritos/listar?page={page}&pageSize={pageSize}";
            if (!string.IsNullOrWhiteSpace(filtro))
                query += $"&filtro={Uri.EscapeDataString(filtro)}";

            using var response = await _sender.SendAsync(HttpMethod.Get, query);
            if (!response.IsSuccessStatusCode) return new();

            return await response.Content.ReadFromJsonAsync<ResultadoPaginadoDto<EscritoDto>>() ?? new();
        }

        public async Task<EscritoDto?> ObtenerEscritoPorIdAsync(int id)
        {
            using var response = await _sender.SendAsync(HttpMethod.Get, $"api/escritos/{id}");
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<EscritoDto>() : null;
        }

        public async Task<bool> GuardarEscritoAsync(string titulo, string html)
        {
            var dto = new { titulo, contenidoHtml = html };
            using var response = await _sender.SendAsync(HttpMethod.Post, "api/escritos/guardar", JsonContent.Create(dto));
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ActualizarEscritoAsync(int id, string titulo, string html)
        {
            var dto = new { titulo, contenidoHtml = html };
            using var response = await _sender.SendAsync(HttpMethod.Patch, $"api/escritos/{id}", JsonContent.Create(dto));
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> EliminarEscritoAsync(int id)
        {
            using var response = await _sender.SendAsync(HttpMethod.Delete, $"api/escritos/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
