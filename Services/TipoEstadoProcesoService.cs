using IurixBlazor.Shared.Config;
using IurixBlazor.Shared.Dtos;
using System.Net.Http.Json;

namespace IurixBlazor.Services
{
    public class TipoEstadoProcesoService
    {
        private readonly HttpClient _httpClient;

        public TipoEstadoProcesoService(IHttpClientFactory factory, ConfigService config)
        {
            //var baseUrl = $"{(config.UsaHttpsServidorBackend ? "https" : "http")}://{config.IpServidor}:{config.Puerto}/";
            //_httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _httpClient = factory.CreateClient("Api");
        }

        public async Task<List<TipoEstadoProcesoDto>> ObtenerTodosAsync()
            => await _httpClient.GetFromJsonAsync<List<TipoEstadoProcesoDto>>("api/TipoEstadoProceso") ?? new();

        public async Task<TipoEstadoProcesoDto?> ObtenerPorIdAsync(int id)
            => await _httpClient.GetFromJsonAsync<TipoEstadoProcesoDto>($"api/TipoEstadoProceso/{id}");

        public async Task<List<TipoEstadoProcesoDto>> ObtenerPorTipoAsync(string tipo)
    => await _httpClient.GetFromJsonAsync<List<TipoEstadoProcesoDto>>($"api/TipoEstadoProceso/por-tipo/{tipo}") ?? new();


        public async Task CrearAsync(TipoEstadoProcesoDto dto)
            => await _httpClient.PostAsJsonAsync("api/TipoEstadoProceso", dto);

        public async Task ActualizarAsync(int id, TipoEstadoProcesoDto dto)
            => await _httpClient.PatchAsync($"api/TipoEstadoProceso/{id}", JsonContent.Create(dto));

        public async Task EliminarAsync(int id)
            => await _httpClient.DeleteAsync($"api/TipoEstadoProceso/{id}");


    }
}
