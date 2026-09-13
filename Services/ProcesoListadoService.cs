using IurixBlazor.Shared.Dtos;
using System.Net.Http;

namespace IurixBlazor.Services
{
    public class ProcesoListadoService
    {
        private readonly HttpClient _http;

        public ProcesoListadoService(IHttpClientFactory httpClientFactory) => _http = httpClientFactory.CreateClient("Api");

        public async Task<PagedResultDto<ProcesoListadoItemDto>> BuscarAsync(ListadoProcesosFiltroDto filtro)
        {
            var resp = await _http.PostAsJsonAsync("api/ProcesoListado/buscar", filtro);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<PagedResultDto<ProcesoListadoItemDto>>()
                   ?? new PagedResultDto<ProcesoListadoItemDto>();
        }

        // ✅ Export PDF: devuelve bytes
        public async Task<byte[]> ExportPdfAsync(ListadoProcesosFiltroDto filtro)
        {
            var resp = await _http.PostAsJsonAsync("api/ProcesoListado/export/pdf", filtro);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadAsByteArrayAsync();
        }
    }

}
