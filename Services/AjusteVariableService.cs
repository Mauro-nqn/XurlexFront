using IurixBlazor.Pages;
using IurixBlazor.Services.Http;
using IurixBlazor.Shared.Dtos;
using System.Net.Http;

namespace IurixBlazor.Services
{
    public class AjusteVariableService
    {
        private readonly IApiRequestSender _sender;
        public AjusteVariableService(IApiRequestSender sender)
        {
            //var baseUrl = $"{(config.UsaHttpsServidorBackend ? "https" : "http")}://{config.IpServidor}:{config.Puerto}/";
            //_http = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _sender = sender;
        }

        public async Task<List<AjusteVariableDto>?> ObtenerTodosAsync()
        {
            using var response = await _sender.SendAsync(HttpMethod.Get, "api/AjustesVariables");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<AjusteVariableDto>>();
        }

        public async Task<AjusteVariableDto?> ObtenerPorIdAsync(int id)
        {
            using var response = await _sender.SendAsync(HttpMethod.Get, $"api/AjustesVariables/{id}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<AjusteVariableDto>();
        }

        public async Task CrearAsync(CrearAjusteVariableDto dto)
        {
            using var r = await _sender.SendAsync(HttpMethod.Post, "api/AjustesVariables", JsonContent.Create(dto));
            r.EnsureSuccessStatusCode();
        }

        public async Task ActualizarAsync(int id, PatchAjusteVariableDto dto)
        {
            using var r = await _sender.SendAsync(HttpMethod.Patch, $"api/AjustesVariables/{id}", JsonContent.Create(dto));
            r.EnsureSuccessStatusCode();
        }

        public async Task EliminarAsync(int id)
        {
            using var r = await _sender.SendAsync(HttpMethod.Delete, $"api/AjustesVariables/{id}");
            r.EnsureSuccessStatusCode();
        }


        public async Task<List<AjusteVariableHistDto>?> ObtenerHistorialAsync(int id, int? take = null)
        {
            using var response = await _sender.SendAsync(HttpMethod.Get,
                $"api/AjustesVariables/{id}/historial{(take is null ? "" : $"?take={take}")}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<AjusteVariableHistDto>>();
        }
    }



}
