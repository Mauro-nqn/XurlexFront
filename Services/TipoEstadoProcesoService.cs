using IurixBlazor.Services.Http;
using IurixBlazor.Shared.Dtos;
using System.Net.Http.Json;

namespace IurixBlazor.Services
{
    public class TipoEstadoProcesoService
    {
        private readonly IApiRequestSender _sender;

        public TipoEstadoProcesoService(IApiRequestSender sender)
        {
            //var baseUrl = $"{(config.UsaHttpsServidorBackend ? "https" : "http")}://{config.IpServidor}:{config.Puerto}/";
            //_httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _sender = sender;
        }

        public async Task<List<TipoEstadoProcesoDto>> ObtenerTodosAsync()
        {
            using var response = await _sender.SendAsync(HttpMethod.Get, "api/TipoEstadoProceso");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<TipoEstadoProcesoDto>>() ?? new();
        }

        public async Task<TipoEstadoProcesoDto?> ObtenerPorIdAsync(int id)
        {
            using var response = await _sender.SendAsync(HttpMethod.Get, $"api/TipoEstadoProceso/{id}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TipoEstadoProcesoDto>();
        }

        public async Task<List<TipoEstadoProcesoDto>> ObtenerPorTipoAsync(string tipo)
        {
            using var response = await _sender.SendAsync(HttpMethod.Get, $"api/TipoEstadoProceso/por-tipo/{tipo}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<TipoEstadoProcesoDto>>() ?? new();
        }


        public async Task CrearAsync(TipoEstadoProcesoDto dto)
        {
            using var response = await _sender.SendAsync(HttpMethod.Post, "api/TipoEstadoProceso", JsonContent.Create(dto));
        }

        public async Task ActualizarAsync(int id, TipoEstadoProcesoDto dto)
        {
            using var response = await _sender.SendAsync(HttpMethod.Patch, $"api/TipoEstadoProceso/{id}", JsonContent.Create(dto));
        }

        public async Task EliminarAsync(int id)
        {
            using var response = await _sender.SendAsync(HttpMethod.Delete, $"api/TipoEstadoProceso/{id}");
        }


    }
}
