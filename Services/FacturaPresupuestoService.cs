using IurixBlazor.Pages;
using IurixBlazor.Shared.Config;
using IurixBlazor.Shared.Dtos;
using System.Net.Http;


public class FacturaPresupuestoService
{
    private readonly HttpClient _httpClient;

    public FacturaPresupuestoService(IHttpClientFactory factory, ConfigService config)
    {
        //var baseUrl = $"{(config.UsaHttpsServidorBackend ? "https" : "http")}://{config.IpServidor}:{config.Puerto}/";
        //_httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
        _httpClient = factory.CreateClient("Api");
    }

    public async Task<List<FacturaResumenDto>> ObtenerPorPresupuestoAsync(int presupuestoId)
    {
        var resp = await _httpClient.GetAsync($"api/FacturaPresupuesto/por-presupuesto/{presupuestoId}");
        resp.EnsureSuccessStatusCode();
        var data = await resp.Content.ReadFromJsonAsync<List<FacturaResumenDto>>();
        return data ?? new List<FacturaResumenDto>();
    }
}
