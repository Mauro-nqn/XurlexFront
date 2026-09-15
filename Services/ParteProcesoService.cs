using IurixBlazor.Shared.Dtos;
using IurixBlazor.Services.Http;
using System.Net.Http;
using System.Net.Http.Json;

namespace IurixBlazor.Services
{
    public class ParteProcesoService
    {
        private readonly IApiRequestSender _sender;



        public ParteProcesoService(IApiRequestSender sender)
        {
            //_http = http;
            _sender = sender;
        }

        // === OBTENER PARTES POR PROCESO JUDICIAL ===
        public async Task<List<ParteProcesoDto>> ObtenerPorProcesoJudicialAsync(int procesoJudicialId)
        {
            using var response = await _sender.SendAsync(HttpMethod.Get, $"api/partes-proceso/por-proceso-judicial/{procesoJudicialId}");
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<List<ParteProcesoDto>>();

            return result ?? new List<ParteProcesoDto>();
        }

        // === OBTENER PARTES POR PROCESO EXTRAJUDICIAL ===
        public async Task<List<ParteProcesoDto>> ObtenerPorProcesoExtrajudicialAsync(int procesoExtrajudicialId)
        {
            using var response = await _sender.SendAsync(HttpMethod.Get, $"api/partes-proceso/por-proceso-extrajudicial/{procesoExtrajudicialId}");
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<List<ParteProcesoDto>>();

            return result ?? new List<ParteProcesoDto>();
        }

        // === CREAR PARTE ===
        public async Task CrearAsync(CrearParteProcesoDto dto)
        {
            using var response = await _sender.SendAsync(HttpMethod.Post, "api/partes-proceso", JsonContent.Create(dto));
            response.EnsureSuccessStatusCode();
        }

        // === ELIMINAR PARTE ===
        public async Task EliminarAsync(int id)
        {
            using var response = await _sender.SendAsync(HttpMethod.Delete, $"api/partes-proceso/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}
