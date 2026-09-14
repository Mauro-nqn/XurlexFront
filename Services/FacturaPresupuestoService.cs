using IurixBlazor.Services.Http;
using IurixBlazor.Shared.Dtos;
using System.Net.Http;


public class FacturaPresupuestoService
{
    private readonly IApiRequestSender _sender;

    public FacturaPresupuestoService(IApiRequestSender sender)
    {
        _sender = sender;
    }

    public async Task<List<FacturaResumenDto>> ObtenerPorPresupuestoAsync(int presupuestoId)
    {
        using var resp = await _sender.SendAsync(HttpMethod.Get, $"api/FacturaPresupuesto/por-presupuesto/{presupuestoId}");
        resp.EnsureSuccessStatusCode();
        var data = await resp.Content.ReadFromJsonAsync<List<FacturaResumenDto>>();
        return data ?? new List<FacturaResumenDto>();
    }
}
