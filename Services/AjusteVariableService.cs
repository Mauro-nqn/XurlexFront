using IurixBlazor.Pages;
using IurixBlazor.Shared.Config;
using IurixBlazor.Shared.Dtos;
using System.Net.Http;

namespace IurixBlazor.Services
{
    public class AjusteVariableService
    {
        private readonly HttpClient _http;
        public AjusteVariableService(IHttpClientFactory factory, ConfigService config)
        {
            //var baseUrl = $"{(config.UsaHttpsServidorBackend ? "https" : "http")}://{config.IpServidor}:{config.Puerto}/";
            //_http = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _http = factory.CreateClient("Api");
        }

        public Task<List<AjusteVariableDto>?> ObtenerTodosAsync()
            => _http.GetFromJsonAsync<List<AjusteVariableDto>>("api/AjustesVariables");

        public Task<AjusteVariableDto?> ObtenerPorIdAsync(int id)
            => _http.GetFromJsonAsync<AjusteVariableDto>($"api/AjustesVariables/{id}");

        public async Task CrearAsync(CrearAjusteVariableDto dto)
        {
            var r = await _http.PostAsJsonAsync("api/AjustesVariables", dto);
            r.EnsureSuccessStatusCode();
        }

        public async Task ActualizarAsync(int id, PatchAjusteVariableDto dto)
        {
            var r = await _http.PatchAsJsonAsync($"api/AjustesVariables/{id}", dto);
            r.EnsureSuccessStatusCode();
        }

        public async Task EliminarAsync(int id)
        {
            var r = await _http.DeleteAsync($"api/AjustesVariables/{id}");
            r.EnsureSuccessStatusCode();
        }


        public Task<List<AjusteVariableHistDto>?> ObtenerHistorialAsync(int id, int? take = null)
            => _http.GetFromJsonAsync<List<AjusteVariableHistDto>>(
                $"api/AjustesVariables/{id}/historial{(take is null ? "" : $"?take={take}")}");
    }



}
