using IurixBlazor.Shared.Dtos;
using IurixBlazor.Services.Http;
using System.Net.Http;
using System.Net.Http.Json;

namespace IurixBlazor.Services
{
    public class InformeHonorariosService
    {
        private readonly IApiRequestSender _sender;

        public InformeHonorariosService(IApiRequestSender sender)
        {
            _sender = sender;
        }

        public async Task<List<LineaHonorarioDto>> ObtenerInformeAsync(InformeHonorariosFiltroDto filtro)
        {
            using var resp = await _sender.SendAsync(HttpMethod.Post, "api/informes/honorarios", JsonContent.Create(filtro));
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<List<LineaHonorarioDto>>()
                   ?? new List<LineaHonorarioDto>();
        }
    }
}
