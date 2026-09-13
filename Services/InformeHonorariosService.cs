using IurixBlazor.Shared.Dtos;
using System.Net.Http;

namespace IurixBlazor.Services
{
    public class InformeHonorariosService
    {
        private readonly HttpClient _httpClient;

        public InformeHonorariosService(IHttpClientFactory factory)
        {
            _httpClient = factory.CreateClient("Api");
        }

        public async Task<List<LineaHonorarioDto>> ObtenerInformeAsync(InformeHonorariosFiltroDto filtro)
        {
            var resp = await _httpClient.PostAsJsonAsync("api/informes/honorarios", filtro);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<List<LineaHonorarioDto>>()
                   ?? new List<LineaHonorarioDto>();
        }
    }
}
