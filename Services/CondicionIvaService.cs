using IurixBlazor.Pages;
using IurixBlazor.Services.Http;
using IurixBlazor.Shared.Dtos;
using System.Net.Http.Json;
using System.Text.Json;

namespace IurixBlazor.Services
{
    public class CondicionIvaService
    {
        private readonly IApiRequestSender _sender;

        public CondicionIvaService(IApiRequestSender sender)
        {
            //var baseUrl = $"{(config.UsaHttpsServidorBackend ? "https" : "http")}://{config.IpServidor}:{config.Puerto}/";
            //_httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _sender = sender;
        }

        /// <summary>
        /// Obtiene todas las condiciones de IVA
        /// </summary>
        public async Task<List<CondicionIvaDto>> ObtenerTodasAsync()
        {
            using var response = await _sender.SendAsync(HttpMethod.Get, "api/CondicionIva");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<CondicionIvaDto>>() ?? new();
        }

        /// <summary>
        /// Obtiene una condición de IVA por ID
        /// </summary>
        public async Task<CondicionIvaDto?> ObtenerPorIdAsync(int id)
        {
            using var response = await _sender.SendAsync(HttpMethod.Get, $"api/CondicionIva/{id}");
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
            using var response = await _sender.SendAsync(HttpMethod.Post, "api/CondicionIva", JsonContent.Create(dto));
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Actualiza una condición de IVA existente
        /// </summary>
        public async Task ActualizarAsync(int id, CondicionIvaDto dto)
        {
            using var response = await _sender.SendAsync(HttpMethod.Patch, $"api/CondicionIva/{id}", JsonContent.Create(dto));
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Elimina una condición de IVA por ID
        /// </summary>
        public async Task EliminarAsync(int id)
        {
            using var response = await _sender.SendAsync(HttpMethod.Delete, $"api/CondicionIva/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}
