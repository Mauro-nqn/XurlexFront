using IurixBlazor.Pages;
using IurixBlazor.Services.Http;
using IurixBlazor.Shared.Dtos;
using System.Net.Http.Json;
using System.Text.Json;

namespace IurixBlazor.Services
{
    public class IvaAlicuotaService
    {
        private readonly IApiRequestSender _sender;

        public IvaAlicuotaService(IApiRequestSender sender)
        {
            //var baseUrl = $"{(config.UsaHttpsServidorBackend ? "https" : "http")}://{config.IpServidor}:{config.Puerto}/";
            //_httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _sender = sender;
        }

        public async Task<List<IvaAlicuotaDto>> ObtenerTodasAsync()
        {
            using var response = await _sender.SendAsync(HttpMethod.Get, "api/IvaAlicuota");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<IvaAlicuotaDto>>() ?? new();
        }

        public async Task<IvaAlicuotaDto?> ObtenerPorIdAsync(int id)
        {
            using var response = await _sender.SendAsync(HttpMethod.Get, $"api/IvaAlicuota/{id}");
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<IvaAlicuotaDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        //Obtener por CodigoAfip
        public async Task<IvaAlicuotaDto?> ObtenerPorCodigoAfipAsync(int codigoAfip)
        {
            using var response = await _sender.SendAsync(HttpMethod.Get, $"api/ivAlicuota/codigoAfip/{codigoAfip}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IvaAlicuotaDto>();
        }

        public async Task CrearAsync(IvaAlicuotaDto dto)
        {
            using var response = await _sender.SendAsync(HttpMethod.Post, "api/IvaAlicuota", JsonContent.Create(dto));
            response.EnsureSuccessStatusCode();
        }

        public async Task ActualizarAsync(int id, IvaAlicuotaDto dto)
        {
            using var response = await _sender.SendAsync(HttpMethod.Patch, $"api/IvaAlicuota/{id}", JsonContent.Create(dto));
            response.EnsureSuccessStatusCode();
        }

        public async Task EliminarAsync(int id)
        {
            using var response = await _sender.SendAsync(HttpMethod.Delete, $"api/IvaAlicuota/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}
