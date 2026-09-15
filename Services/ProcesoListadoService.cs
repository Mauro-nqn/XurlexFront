using IurixBlazor.Shared.Dtos;
using System.Net.Http;
using System.Net.Http.Json;
using IurixBlazor.Services.Http;

namespace IurixBlazor.Services
{
    public class ProcesoListadoService
    {
        private readonly IApiRequestSender _sender;

        public ProcesoListadoService(IApiRequestSender sender) => _sender = sender;

        public async Task<PagedResultDto<ProcesoListadoItemDto>> BuscarAsync(ListadoProcesosFiltroDto filtro)
        {
            using var resp = await _sender.SendAsync(HttpMethod.Post, "api/ProcesoListado/buscar", JsonContent.Create(filtro));
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<PagedResultDto<ProcesoListadoItemDto>>()
                   ?? new PagedResultDto<ProcesoListadoItemDto>();
        }

        // ✅ Export PDF: devuelve bytes
        public async Task<byte[]> ExportPdfAsync(ListadoProcesosFiltroDto filtro)
        {
            using var resp = await _sender.SendAsync(HttpMethod.Post, "api/ProcesoListado/export/pdf", JsonContent.Create(filtro));
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadAsByteArrayAsync();
        }
    }

}
