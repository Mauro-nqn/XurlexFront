using IurixBlazor.Pages;
using IurixBlazor.Services.Http;
using IurixBlazor.Shared.Dtos;
using System.Net.Http.Json;

namespace IurixBlazor.Services
{
    public class TipoComprobanteService
    {
        private readonly IApiRequestSender _sender;

        public TipoComprobanteService(IApiRequestSender sender)
        {
            //var baseUrl = $"{(config.UsaHttpsServidorBackend ? "https" : "http")}://{config.IpServidor}:{config.Puerto}/";
            //_httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _sender = sender;
        }

        public async Task<List<TipoComprobanteDto>> ObtenerTodosAsync()
        {
            using var response = await _sender.SendAsync(HttpMethod.Get, "api/tipocomprobante");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<TipoComprobanteDto>>() ?? new();
        }

        public async Task<TipoComprobanteDto?> ObtenerPorCodigoAsync(int codigo)
        {
            using var response = await _sender.SendAsync(HttpMethod.Get, $"api/tipocomprobante/{codigo}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TipoComprobanteDto>();
        }

        public async Task CrearAsync(CrearTipoComprobanteDto dto)
        {
            using var response = await _sender.SendAsync(HttpMethod.Post, "api/tipocomprobante", JsonContent.Create(dto));
            response.EnsureSuccessStatusCode();
        }

        public async Task ActualizarAsync(int codigo, CrearTipoComprobanteDto dto)
        {
            using var response = await _sender.SendAsync(HttpMethod.Patch, $"api/tipocomprobante/{codigo}", JsonContent.Create(dto));
            response.EnsureSuccessStatusCode();
        }

        public async Task EliminarAsync(int codigo)
        {
            using var response = await _sender.SendAsync(HttpMethod.Delete, $"api/tipocomprobante/{codigo}");
            response.EnsureSuccessStatusCode();
        }
    }
}

