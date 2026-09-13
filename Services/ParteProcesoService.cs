using IurixBlazor.Shared.Dtos;
using System.Net.Http;
using System.Net.Http.Json;

namespace IurixBlazor.Services
{
    public class ParteProcesoService
    {
        private readonly HttpClient _http;



        public ParteProcesoService(IHttpClientFactory factory)
        {
            //_http = http;
            _http = factory.CreateClient("Api");
        }

        // === OBTENER PARTES POR PROCESO JUDICIAL ===
        public async Task<List<ParteProcesoDto>> ObtenerPorProcesoJudicialAsync(int procesoJudicialId)
        {
            var result = await _http.GetFromJsonAsync<List<ParteProcesoDto>>(
                $"api/partes-proceso/por-proceso-judicial/{procesoJudicialId}");

            return result ?? new List<ParteProcesoDto>();
        }

        // === OBTENER PARTES POR PROCESO EXTRAJUDICIAL ===
        public async Task<List<ParteProcesoDto>> ObtenerPorProcesoExtrajudicialAsync(int procesoExtrajudicialId)
        {
            var result = await _http.GetFromJsonAsync<List<ParteProcesoDto>>(
                $"api/partes-proceso/por-proceso-extrajudicial/{procesoExtrajudicialId}");

            return result ?? new List<ParteProcesoDto>();
        }

        // === CREAR PARTE ===
        public async Task CrearAsync(CrearParteProcesoDto dto)
        {
            var response = await _http.PostAsJsonAsync("api/partes-proceso", dto);
            response.EnsureSuccessStatusCode();
        }

        // === ELIMINAR PARTE ===
        public async Task EliminarAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/partes-proceso/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}
